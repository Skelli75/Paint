using System;
using System.Collections.Generic;
using System.Diagnostics.SymbolStore;
using System.Windows;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;

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
        public Stroke DrawEllipse(Point start, Point end, int pointsCount, Color color, double sizeValue)
        {
            // Calculate center, width, and height of the bounding box
            double centerX = (start.X + end.X) / 2;
            double centerY = (start.Y + end.Y) / 2;
            double width = Math.Abs(end.X - start.X);
            double height = Math.Abs(end.Y - start.Y);

            StylusPointCollection pts = new StylusPointCollection();

            // Generate points around the ellipse
            for (int i = 0; i < pointsCount; i++)
            {
                double angle = (double)i / pointsCount * 2 * Math.PI;
                double x = centerX + (width / 2) * Math.Cos(angle);
                double y = centerY + (height / 2) * Math.Sin(angle);

                pts.Add(new StylusPoint(x, y));
            }

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

