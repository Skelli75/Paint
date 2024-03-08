using System.Collections.Generic;
using System.Diagnostics.SymbolStore;
using System.Windows;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Paint
{
    internal class ShapeTools
    {
        public ShapeTools() 
        {
            
        }
        public Stroke DrawLine(Point start, Point end, Color color, double sizeValue) // metoden ritar både ut finalstrokes och visualstrokes
        {
            StylusPointCollection pts = new();
            pts.Add(new StylusPoint(start.X, start.Y));
            pts.Add(new StylusPoint(end.X, end.Y));

            DrawingAttributes dA = new()
            {
                Color = color,
                Height = sizeValue,
                Width = sizeValue,
            };

            Stroke stroke = new(pts, dA);

            return stroke;
        }

        public Stroke DrawRectangle(Point start, Point end, Color color, double sizeValue)  // när man har datan
        {
            StylusPointCollection pts = new();

            pts.Add(new StylusPoint(start.X, start.Y));
            pts.Add(new StylusPoint(start.X, end.Y));
            pts.Add(new StylusPoint(end.X, end.Y));
            pts.Add(new StylusPoint(end.X, start.Y));
            pts.Add(new StylusPoint(start.X, start.Y));

            DrawingAttributes dA = new()
            {
                Color = color,
                Height = sizeValue,
                Width = sizeValue,
                StylusTip = StylusTip.Rectangle
            };

            Stroke stroke = new(pts, dA);

            return stroke;
        }
    }
}

