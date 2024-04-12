using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Paint
{
    internal class Tools
    {
        Dictionary<string, DrawingAttributes> _toolPresets; //A dictionary is used to store the different tools' DrawingAttributes
        InkCanvas _canvas;
        public DrawTools _drawTools;

        string _currentTool; //has the current tool for reference
        


        public Tools(InkCanvas canvas)
        {
            _toolPresets = new Dictionary<string, DrawingAttributes>()
            {
                { "pen", new DrawingAttributes() { Color = Colors.Black } },
                { "eraser", new DrawingAttributes() { Color = Colors.White} },
                { "colorPicker", new DrawingAttributes(){ Color = Colors.Transparent } },
                { "line", new DrawingAttributes() { Color = Colors.Transparent } },
                { "rectangle", new DrawingAttributes() { Color = Colors.Transparent } },
                { "ellipse", new DrawingAttributes() { Color = Colors.Transparent, StylusTip = StylusTip.Ellipse } },
                { "fill", new DrawingAttributes() { Color = Colors.Transparent } }
            };

            _drawTools = new();
            _canvas = canvas;
            _currentTool = "pen";
        }

        public void SetColor(System.Windows.Media.Color color) //Give new color to pen
        {
            _toolPresets["pen"].Color = color; //It is only the pen that needs its DrawingAttribute.Color changed as the other tools don't rely on this for color change
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

        public void SaveCanvas() //Saves the canvas as a JPeg
        {
            //Starts a saveFileDialog that lets the user choose where to save the image
            SaveFileDialog save = new()
            {
                Filter = "JPeg Image|*.jpg",
                Title = "Save an Image File",
                FileName = "default"
            };
            if (save.ShowDialog() == true)
            {
                //Renders the canvas as a bitmap and saves it as a JPeg
                Bitmap bitmap = RenderTargetBitmap();
                bitmap.Save(save.FileName);
            }
        }

        public bool LoadImageFromFile()
        {
            //Starts a openFileDialog that lets the user open a JPeg
            OpenFileDialog open = new OpenFileDialog();
            open.Filter = "JPeg Image|*.jpg";
            open.ShowDialog();

            try
            {
                //Converts the choosen JPeg as an Image
                Stream fileStream = open.OpenFile();
                System.Drawing.Image fileContent = System.Drawing.Image.FromStream(fileStream);

                //Converts the Image to a bitmap
                Bitmap bitmap = new(fileContent);

                //Loads the bitmap to the canvas as a background  
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

            RenderTargetBitmap rtb = new(width, height, 96, 96, PixelFormats.Default);

            rtb.Render(_canvas);
            BmpBitmapEncoder encoder = new();
            encoder.Frames.Add(BitmapFrame.Create(rtb));
            rtb.Render(_canvas);

            MemoryStream ms = new MemoryStream();
            encoder.Save(ms);

            return new Bitmap(ms);
        }

        public BitmapImage BitmapToBitmapImage(Bitmap bitmap) //Converts a bitmap to a bitmapImage 
        {
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, ImageFormat.Png);
                memory.Position = 0;

                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                bitmapImage.Freeze();

                return bitmapImage;
            }
        }
        public void LoadBitmaptoCanvas(Bitmap bitmap) //Loads a bitmap to the canvas using an ImageBrush
        {
            ImageBrush ib = new();
            ib.ImageSource = BitmapToBitmapImage(bitmap);

            _canvas.Background = ib;
        }
    }
}
