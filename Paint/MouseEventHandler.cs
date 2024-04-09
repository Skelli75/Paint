using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Media;
using System.Windows;

namespace Paint
{
    internal class MouseEventHandler
    {
        Tools _tools;

        public MouseEventHandler(Tools tools) 
        { 
            _tools = tools; 
        }


        public Stroke MouseMove(Point start, Point end, Color color, double size, InkCanvas canvas)
        { 
            Stroke newStroke = _tools._drawTools.DrawLine(new System.Windows.Point(0, 0), new System.Windows.Point(0, 0), Colors.Transparent, size); // ritar ut en linje som sedan ska ändras
            

            if (canvas.DefaultDrawingAttributes == _tools.GetTool("line"))
            {
                newStroke = _tools._drawTools.DrawLine(start, end, color, size); ; // skapar newLine
            }
            else if (canvas.DefaultDrawingAttributes == _tools.GetTool("rectangle"))
            {
                newStroke = _tools._drawTools.DrawRectangle(start, end, color, size);
            }
            else if (canvas.DefaultDrawingAttributes == _tools.GetTool("ellipse"))
            {
                newStroke = _tools._drawTools.DrawEllipse(start, end, 500, color, size);
            }

            return newStroke;   
        }

    }
}
