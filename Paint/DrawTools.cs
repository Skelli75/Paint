using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;

namespace Paint
{
    internal class DrawTools
    {
        public DrawTools() 
        {
            
        }
        public StrokeCollection DrawLine(Point start, Point end, Color color, double sizeValue) //Draws a line between start and end 
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

            StrokeCollection newStroke = new() { new Stroke(pts, dA) };

            return newStroke;
        }

        public StrokeCollection DrawRectangle(Point start, Point end, Color color, double sizeValue)  //Calculates where the points of the rectangle should be with the help of start and end and then draws between them 
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

            StrokeCollection newStroke = new() { new Stroke(pts, dA) };

            return newStroke;
        }
        public StrokeCollection DrawEllipse(Point start, Point end, int pointsCount, Color color, double sizeValue) //Places points around a circle between start and end then generates a stroke from that
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

            StrokeCollection newStroke = new() { new Stroke(pts, dA) };

            return newStroke;
        }

        public StrokeCollection FloodFill(System.Drawing.Bitmap bmp, Point pt, System.Drawing.Color targetColor, System.Drawing.Color replacementColor, double sizeValue) //Fills a freeform drawn shape with color. Currently has a bug where a line appears whenever a jump in the scan occurs.
        {
            StylusPointCollection pts = new();

            targetColor = bmp.GetPixel((int)pt.X, (int)pt.Y);
            if (targetColor.ToArgb().Equals(replacementColor.ToArgb()))
            {
                return null;
            }

            Stack<Point> pixels = new Stack<Point>();

            pixels.Push(pt);
            while (pixels.Count != 0)

            {
                System.Windows.Point temp = pixels.Pop();
                int y1 = (int)temp.Y;
                while (y1 >= 0 && bmp.GetPixel((int)temp.X, y1) == targetColor)
                {
                    y1--;
                }
                y1++;
                bool spanLeft = false;
                bool spanRight = false;
                while (y1 < bmp.Height && bmp.GetPixel((int)temp.X, y1) == targetColor)
                {
                    bmp.SetPixel((int)temp.X, y1, replacementColor);
                    pts.Add(new StylusPoint((int)temp.X, y1));


                    if (temp.X > 1 && !spanLeft && bmp.GetPixel((int)temp.X - 1, y1) == targetColor)
                    {
                        pixels.Push(new Point(temp.X - 1, y1));
                        spanLeft = true;
                    }
                    else if (spanLeft && temp.X - 1 == 0 && bmp.GetPixel((int)temp.X - 1, y1) != targetColor)
                    {
                        spanLeft = false;
                    }
                    else if (!spanRight && temp.X < bmp.Width - 1 && bmp.GetPixel((int)temp.X + 1, y1) == targetColor)
                    {
                        pixels.Push(new Point(temp.X + 1, y1));
                        spanRight = true;
                    }
                    else if (spanRight && temp.X < bmp.Width - 1 && bmp.GetPixel((int)temp.X + 1, y1) != targetColor)
                    {
                        spanRight = false;
                    }
                    y1++;
                }
            }

            DrawingAttributes dA = new() 
            { 
                Color = Color.FromRgb(replacementColor.R, replacementColor.G, replacementColor.B), 
                Height = sizeValue, 
                Width = sizeValue, 
                StylusTip = StylusTip.Rectangle 
            };

            StrokeCollection newStroke = new StrokeCollection() { new Stroke(pts, dA) };


            return newStroke;
        }

    }
}

