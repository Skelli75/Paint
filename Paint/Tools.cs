﻿using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics.SymbolStore;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Paint
{
    internal class Tools
    {
        Dictionary<string, DrawingAttributes> _toolPresets;
        InkCanvas _canvas;
        public DrawTools _drawTools;
        string _currentTool;

        public Tools(InkCanvas canvas)
        {
            _toolPresets = new Dictionary<string, DrawingAttributes>()
            {
                { "pen", new DrawingAttributes() { Color = Colors.Black } },
                { "eraser", new DrawingAttributes() { Color = Colors.White} },
                { "colorPicker", new DrawingAttributes(){ Color = Colors.Transparent } },
                { "line", new DrawingAttributes() { Color = Colors.Transparent, Width = 1 } },
                { "rectangle", new DrawingAttributes() { Color = Colors.Transparent, Width = 2 } },
                { "ellipse", new DrawingAttributes() { Color = Colors.Transparent, Width = 3, StylusTip = StylusTip.Ellipse } },
                { "fill", new DrawingAttributes() { Color = Colors.Transparent } }
            };

            _drawTools = new();
            _canvas = canvas;
            _currentTool = "pen";
        }

        public void SetColor(System.Windows.Media.Color color)
        {
            _toolPresets["pen"].Color = color;
        }

        public void SetTool(string tool)
        {
            _canvas.DefaultDrawingAttributes = _toolPresets[tool];
            _currentTool = tool;
        }

        public string GetTool()
        {
            return _currentTool;
        }

        public void SaveCanvas() //Sparar ner canvas bilden där man vill
        {
            //Startar en saveFileDialog för att låta användaren själv välja vart bilden ska sparas
            SaveFileDialog save = new()
            {
                Filter = "JPeg Image|*.jpg",
                Title = "Save an Image File",
                FileName = "default"
            };
            if (save.ShowDialog() == true)
            {
                //omvandlar inkcanvas:en till en bitmap och sparar sedan ner den som en jpg
                Bitmap bitmap = RenderTargetBitmap();
                bitmap.Save(save.FileName);
            }
        }

        public bool LoadImageFromFile()
        {
            //Startar OpenFileDialog för att användaren ska kunna välja en jpg 
            OpenFileDialog open = new OpenFileDialog();
            open.Filter = "JPeg Image|*.jpg";
            open.ShowDialog();

            try
            {
                //sparar jpg:en som en image 
                Stream fileStream = open.OpenFile();
                System.Drawing.Image fileContent = System.Drawing.Image.FromStream(fileStream);

                //Omvandlar Image filecontent till en bitmap
                Bitmap bitmap = new(fileContent);

                //Laddar upp bitmapen på canvas:en
                LoadBitmaptoCanvas(bitmap);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public Bitmap RenderTargetBitmap() //Renders the canvas to a RenderTargetBitmap
        {
            int width = (int)_canvas.ActualWidth;
            int height = (int)_canvas.ActualHeight;

            RenderTargetBitmap rtb = new(width, height, 96, 96, System.Windows.Media.PixelFormats.Default);
            rtb.Render(_canvas);
            BmpBitmapEncoder encoder = new();
            encoder.Frames.Add(BitmapFrame.Create(rtb));
            rtb.Render(_canvas);

            MemoryStream ms = new MemoryStream();
            encoder.Save(ms);

            return new Bitmap(ms);
        }

        public BitmapImage BitmapToBitmapImage(Bitmap bitmap) //Omvandlar en bitmap till en bitmapimage
        {
            using (var memory = new MemoryStream())
            {
                bitmap.Save(memory, ImageFormat.Png);
                memory.Position = 0;

                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                bitmapImage.Freeze();

                return bitmapImage;
            }
        }
        public void LoadBitmaptoCanvas(Bitmap bitmap) //Laddar in en bitmap på canvas:en
        {
            ImageBrush ib = new();
            ib.ImageSource = BitmapToBitmapImage(bitmap);

            _canvas.Background = ib;
        }
    }
}
