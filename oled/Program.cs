using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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
/// 
namespace oled
{
    class Program
    {
        static void Main(string[] args)
        {

            using (UxSSD1306 _1306 = new UxSSD1306())
            {
              
                Console.WriteLine("Initilize Oled");
                _1306.Initialize();
                Graphics display = _1306.GetGraphics();
                display.DrawRectangle(Pens.Black, 0, 0, _1306.DisplayWidth-1, _1306.DisplayHeight-1);
                display.DrawEllipse(Pens.Black, 10, 10, _1306.DisplayWidth-20, _1306.DisplayHeight-20);
                Console.WriteLine("Draw Oled");
                _1306.Update(display);
                Console.WriteLine("Wait 10 sec");
                Thread.Sleep(10000);
                display = _1306.GetGraphics();
                _1306.Update(display);
                Console.WriteLine("Clear display");
                Console.WriteLine("Wait 1 sec");
                Thread.Sleep(1000);

            }
        }
    }
}
