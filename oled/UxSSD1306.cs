using Mono.Unix.Native;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;


/// <summary>
///  Author: Matthias Zartmann
///  Date: 06.15.2018
///  Copyright (C) 2018 Matthias Zartmann
///  
///  This program is free software; you can redistribute it and/or modify
///  it under the terms of the GNU General Public License as published by
///  the Free Software Foundation; either version 2 of the License, or
///  (at your option) any later version.
/// 
///  This program is distributed in the hope that it will be useful, 
///  but WITHOUT ANY WARRANTY; without even the implied warranty of
///  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
///  GNU General Public License for more details.
/// 
///  You should have received a copy of the GNU General Public License
///  along with this program; if not, write to the Free Software
///  Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA
/// </summary>

namespace oled
{
    /// <summary>
    /// Class for Oled 0,96" with SSD1306 controller
    /// </summary>
    public class UxSSD1306 : IDisposable
    {
        private const byte DEVICEID = 0x3c;

        private const int DISPLAY_WIDTH = 128;
        private const int DISPLAY_HEIGHT = 64;
        private const int DISPLAY_SIZE = DISPLAY_WIDTH * DISPLAY_HEIGHT / 8;	// 8 pages/lines with 128 
        private const byte CMD_MEMORY_MODE = 0x20;
        private const byte SSD1306_I2C_ADDRESS = 0x3C;
        private const byte SSD1306_SETCONTRAST = 0x81;
        private const byte SSD1306_DISPLAYALLON_RESUME = 0xA4;
        private const byte SSD1306_DISPLAYALLON = 0xA5;
        private const byte SSD1306_NORMALDISPLAY = 0xA6;
        private const byte SSD1306_INVERTDISPLAY = 0xA7;
        private const byte SSD1306_DISPLAYOFF = 0xAE;
        private const byte SSD1306_DISPLAYON = 0xAF;
        private const byte SSD1306_SETDISPLAYOFFSET = 0xD3;
        private const byte SSD1306_SETCOMPINS = 0xDA;
        private const byte SSD1306_SETVCOMDETECT = 0xDB;
        private const byte SSD1306_SETDISPLAYCLOCKDIV = 0xD5;
        private const byte SSD1306_SETPRECHARGE = 0xD9;
        private const byte SSD1306_SETMULTIPLEX = 0xA8;
        private const byte SSD1306_SETLOWCOLUMN = 0x00;
        private const byte SSD1306_SETHIGHCOLUMN = 0x10;
        private const byte SSD1306_SETSTARTLINE = 0x40;
        private const byte SSD1306_MEMORYMODE = 0x20;
        private const byte SSD1306_COLUMNADDR = 0x21;
        private const byte SSD1306_PAGEADDR = 0x22;
        private const byte SSD1306_COMSCANINC = 0xC0;
        private const byte SSD1306_COMSCANDEC = 0xC8;
        private const byte SSD1306_SEGREMAP = 0xA0;
        private const byte SSD1306_CHARGEPUMP = 0x8D;
        private const byte SSD1306_EXTERNALVCC = 0x1;
        private const byte SSD1306_SWITCHCAPVCC = 0x2;

        // Scrolling constants
        private const byte SSD1306_ACTIVATE_SCROLL = 0x2F;
        private const byte SSD1306_DEACTIVATE_SCROLL = 0x2E;
        private const byte SSD1306_SET_VERTICAL_SCROLL_AREA = 0xA3;
        private const byte SSD1306_RIGHT_HORIZONTAL_SCROLL = 0x26;
        private const byte SSD1306_LEFT_HORIZONTAL_SCROLL = 0x27;
        private const byte SSD1306_VERTICAL_AND_RIGHT_HORIZONTAL_SCROLL = 0x29;
        private const byte SSD1306_VERTICAL_AND_LEFT_HORIZONTAL_SCROLL = 0x2A;


        //WiringPi Functions
        [DllImport("libwiringPi.so", EntryPoint = "wiringPiI2CSetup", CallingConvention = CallingConvention.Cdecl)]
        static extern int wiringPiI2CSetup(int pDeviceId);

        [DllImport("libwiringPi.so", EntryPoint = "wiringPiI2CWriteReg8", CallingConvention = CallingConvention.Cdecl)]
        static extern int wiringPiI2CWriteReg8(int pHandle, byte pRegister, byte data);


        /// <summary>
        /// the file handle from WiringPi
        /// </summary>
        int _DeviceHandle;

        
        /// <summary>
        /// Image to paint on the OLED
        /// </summary>
        private Bitmap _DisplayImage;


        public UxSSD1306()
        {
            _DeviceHandle = -1;
            _DisplayImage = new Bitmap(DISPLAY_WIDTH, DISPLAY_HEIGHT);
        }


        private void SendCommand(byte pCommand)
        {
            wiringPiI2CWriteReg8(_DeviceHandle, 0x00, pCommand);
        }
        private void SendData(byte pData)
        {
            wiringPiI2CWriteReg8(_DeviceHandle, 0x40, pData);    // 0x40 for data

        }

        private void SendBuffer(byte[] pData)
        {
            SendCommand(SSD1306_COLUMNADDR);
            SendCommand(0); // Column start address (0 = reset)
            SendCommand(DISPLAY_WIDTH - 1);  // Column end address (127 
            SendCommand(SSD1306_PAGEADDR);
            SendCommand(0); // Page start address (0 = reset)
            SendCommand(7); // Page end address
                  

            for (int i = 0; i < pData.Length; i++)
            {

                wiringPiI2CWriteReg8(_DeviceHandle, 0x40, pData[i]);    // 0x40 for data

            }
        }

        /// <summary>
        /// Initialize the Oled see the datasheet https://cdn-shop.adafruit.com/datasheets/SSD1306.pdf
        /// </summary>
        public void Initialize()
        {


            _DeviceHandle = wiringPiI2CSetup(DEVICEID);

            SendCommand(SSD1306_DISPLAYOFF);

            SendCommand(SSD1306_SETDISPLAYCLOCKDIV);            // 0xD5
            SendCommand(0x80);                              // the suggested ratio 0x80
            SendCommand(SSD1306_SETMULTIPLEX);                  // 0xA8
            SendCommand(0x3F);
            SendCommand(SSD1306_SETDISPLAYOFFSET);         // 0xD3
            SendCommand(0x0);                                  //# no offset
            SendCommand(SSD1306_SETSTARTLINE | 0x0);
            SendCommand(SSD1306_CHARGEPUMP);                    //# 0x8D
            SendCommand(0x14);
            SendCommand(SSD1306_MEMORYMODE);  //                  # 0x20
            SendCommand(0x00);               //# 0x0 act like ks0108
            SendCommand(SSD1306_SEGREMAP | 0x1);
            SendCommand(SSD1306_COMSCANDEC);
            SendCommand(SSD1306_SETCOMPINS);            //   # 0xDA
            SendCommand(0x12);
            SendCommand(SSD1306_SETCONTRAST);         //       # 0x81
            SendCommand(0xCF);
            SendCommand(SSD1306_SETPRECHARGE);//                 # 0xd9

            SendCommand(0xF1);//

            SendCommand(SSD1306_SETVCOMDETECT);//                # 0xDB
            SendCommand(0x40);//
            SendCommand(SSD1306_DISPLAYALLON_RESUME);//   # 0xA4
            SendCommand(SSD1306_NORMALDISPLAY);//                # 0xA6

            SendCommand(SSD1306_DEACTIVATE_SCROLL);

            SendCommand(SSD1306_DISPLAYON); // --turn on oled panel


        }

        /// <summary>
        /// Get the Graphics to draw
        /// </summary>
        /// <returns>Graphics to draw your stuff</returns>
        public Graphics GetGraphics()
        {
            Graphics result = Graphics.FromImage(_DisplayImage);

            result.SmoothingMode = SmoothingMode.None;
            result.InterpolationMode = InterpolationMode.HighQualityBicubic;
            result.PixelOffsetMode = PixelOffsetMode.HighQuality;

            result.Clear(Color.White);

            return result;
        }


        /// <summary>
        /// To debug what ar drawed
        /// </summary>
        /// <param name="pFilename"> Imagefilename </param>
        public void SaveAsfile(string pFilename)
        {
            _DisplayImage.Save(pFilename);
        }

        /// <summary>
        /// Update the Oled display
        /// </summary>
        /// <param name="pGraphics">the graphics from GetGraphics() </param>
        public void Update(Graphics pGraphics)
        {
            pGraphics.Flush();


            byte[] _tempbuffer = new byte[DISPLAY_SIZE];

            for (int y = 0; y < DISPLAY_HEIGHT; y++)
            {
                for (int x = 0; x < DISPLAY_WIDTH; x++)
                {
                    Color pixel = _DisplayImage.GetPixel(x, y);
                    if (pixel.GetBrightness() < 0.5)
                    {
                        _tempbuffer[(y / 8) * DISPLAY_WIDTH + x] |= (byte)(1 << (y % 8));
                    }
                }
            }
            SendBuffer(_tempbuffer);
        }


        /// <summary>
        /// The displaywidth in pixel
        /// </summary>
        public int DisplayWidth
        {
            get { return DISPLAY_WIDTH; }
        }
        /// <summary>
        /// the Displayheight in Pixel
        /// </summary>
        public int DisplayHeight
        {
            get { return DISPLAY_HEIGHT; }
        }

        /// <summary>
        /// Free Resources
        /// </summary>
        public void Dispose()
        {
            if (_DeviceHandle > -1)
            {
                //Close the handle
                Syscall.close(_DeviceHandle);

            }

        }
    }
}
