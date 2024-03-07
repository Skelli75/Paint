﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Ink;
using Microsoft.Win32;
using System.IO;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Media.Imaging;

namespace Paint
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<StrokeCollection> _added = new(); //För kunna undo:a strokes måste man komma åt strokes. Detta görs via en lista som fylls med nya strokes.
        private List<StrokeCollection> _removed = new(); //Samma gäller för strokes som tagits bort med hjälp av undo
        private bool handle = true; 
        private Color _color;
        
        /* Definerar drawing tools */
        private readonly DrawingAttributes PenTool = new() 
        {
            Color = System.Windows.Media.Colors.DarkGreen,
            Height = 2,
            Width = 2
        };

        private readonly DrawingAttributes EraserTool = new() 
        {
            Color = System.Windows.Media.Colors.White,
            Height = 4,
            Width = 4
        };

        private readonly DrawingAttributes FillTool = new()
        {
            Color = System.Windows.Media.Colors.Transparent,
        };
        /* Definerar drawing tools */

        public MainWindow()
        {
            InitializeComponent();

            //anger standard drawing tool som pentool och standard färgen som svart
            StandardCanvas.DefaultDrawingAttributes = PenTool;
            StandardCanvas.UseCustomCursor = true;
            _color = Color.Black;

            //Ger sizeslider min och max värden
            SizeSlider.Maximum = 100;
            SizeSlider.Minimum = 4;
            

            StandardCanvas.Strokes.StrokesChanged += Strokes_StrokesChanged;
        }

        private void Strokes_StrokesChanged(object sender, StrokeCollectionChangedEventArgs e)
        {
            if (handle) //ser till så att inga sträck kan läggas till medans undo och redo utförs
            {
                _added.Add(e.Added); //
                _removed.Clear(); // gör så att man inte kan redo:a sträck som togs bort innan det senaste sträcket
            }
        }

        private void Undo(object sender, RoutedEventArgs e)
        {
            handle = false;
            if (_added.Count > 0)
            {
                _removed.Add(_added[_added.Count - 1]); // sparar det senaste ritade sträcket för att senare kunna redo:a om det önskas
                StandardCanvas.Strokes.Remove(_added[_added.Count - 1]); // tar bort det senaste ritade sträcket från tavlan
                _added.Remove(_added[_added.Count - 1]); // tar bort sträcket från "finns på tavlan" listan
            }
            handle = true;
        }

        private void Redo(object sender, RoutedEventArgs e)
        {
            handle = false;
            if (_added.Count >= 0 && _removed.Count > 0)
            {
                _added.Add(_removed[_removed.Count - 1]); // lägger tillbaka det senaste borttagna sträcket i "finns på tavlan" listan
                StandardCanvas.Strokes.Add(_removed[_removed.Count - 1]); // lägger tillbaka sträcket på tavlan
                _removed.Remove(_removed[_removed.Count - 1]); // tar bort sträcket från listan med borttagna sträck
            }
            handle = true;
        }

        /* Buttons */
        private void EraserToolButton(object sender, RoutedEventArgs e)
        {
            StandardCanvas.DefaultDrawingAttributes = EraserTool;
            UpdateSizeSlider();
        }

        private void PencilToolButton(object sender, RoutedEventArgs e)
        {
            StandardCanvas.DefaultDrawingAttributes = PenTool;
            UpdateSizeSlider();
        }

        public void FillToolButton(object sender, RoutedEventArgs e)
        {
            StandardCanvas.DefaultDrawingAttributes = FillTool;
        }

        private void LineToolButton(object sender, RoutedEventArgs e)
        {

        }

        private void BentLineToolButton(object sender, RoutedEventArgs e)
        {

        }

        private void CircleToolButton(object sender, RoutedEventArgs e)
        {

        }

        private void SquareToolButton(object sender, RoutedEventArgs e)
        {

        }

        private void SaveButtonClick(object sender, RoutedEventArgs e)
        {
            SaveCanvas();
        }

        private void RedoButton(object sender, RoutedEventArgs e)
        {
            Redo(sender, e);
        }

        private void UndoButton(object sender, RoutedEventArgs e)
        {
            Undo(sender, e);
        }

        private void LoadButtonClick(object sender, RoutedEventArgs e)
        {
            LoadImageFromFile();
        }
        /* Buttons */

        private void SizeSliderValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) //Tilldelar SizeSliderns value till både height och width på verktygen
        {
            StandardCanvas.DefaultDrawingAttributes.Height = SizeSlider.Value; 
            StandardCanvas.DefaultDrawingAttributes.Width = SizeSlider.Value;
        }

        private void UpdateSizeSlider() //Ändrar SizeSliderns value för att reflektera stroleken på det valda verktyget
        {
            SizeSlider.Value = StandardCanvas.DefaultDrawingAttributes.Width;
        }

        private void SaveCanvas() //Sparar ner canvas bilden där man vill
        {
            //Startar en saveFileDialog för att låta användaren själv välja vart bilden ska sparas
            SaveFileDialog save = new(); 
            save.Filter = "JPeg Image|*.jpg";
            save.Title = "Save an Image File";
            save.FileName = "default";
            save.ShowDialog();

            //omvandlar inkcanvas:en till en bitmap och sparar sedan ner den som en jpg
            Bitmap bitmap = RenderTargetBitmap();
            bitmap.Save(save.FileName);
        }

        private void UpdateWindow(object sender, RoutedEventArgs e) //Uppdaterar storleken av canvas:en när storleken av programmet ändras för att reflektera ändringen
        {
            StandardCanvas.Height = (Paint.Height / 3) * 2 ;
            StandardCanvas.Width = (Paint.Width / 3) * 2;
        }

        private void LoadImageFromFile() 
        {
            //Startar OpenFileDialog för att användaren ska kunna välja en jpg 
            OpenFileDialog open = new OpenFileDialog();
            open.Filter = "JPeg Image|*.jpg";
            open.ShowDialog();
            
            //sparar jpg:en som en image 
            Stream fileStream = open.OpenFile();
            System.Drawing.Image fileContent = System.Drawing.Image.FromStream(fileStream);

            //Omvandlar Image filecontent till en bitmap
            Bitmap bitmap = new(fileContent);

            //Laddar upp bitmapen på canvas:en
            LoadBitmaptoCanvas(bitmap);
        }

        private Color GetColorUnderMouse(MouseEventArgs m) //return:ar färgen där man klickar
        {
            Bitmap bitmap = RenderTargetBitmap();
            Color color = bitmap.GetPixel((int)m.GetPosition(StandardCanvas).X, (int)m.GetPosition(StandardCanvas).Y);

            return color;
        }

        private void CanvasClick(object sender, MouseButtonEventArgs e)
        {
            //kollar vilket verktyg som är aktivt
            if (StandardCanvas.DefaultDrawingAttributes == FillTool) 
            {
                Color targetColor = GetColorUnderMouse(e);
                Color replacementColor = _color;

                if (targetColor != replacementColor)
                {
                    FloodFill(RenderTargetBitmap(), e.GetPosition(StandardCanvas), replacementColor, targetColor);
                }
            }
        }

        private Bitmap RenderTargetBitmap() //Renders the canvas to a RenderTargetBitmap
        {
            RenderTargetBitmap rtb = new((int)StandardCanvas.Width, (int)StandardCanvas.Height, 96, 96, System.Windows.Media.PixelFormats.Default);
            rtb.Render(StandardCanvas);
            BmpBitmapEncoder encoder = new();
            encoder.Frames.Add(BitmapFrame.Create(rtb));
            rtb.Render(StandardCanvas);

            MemoryStream ms = new MemoryStream();
            encoder.Save(ms);

            return new Bitmap(ms);
        }
        private BitmapImage BitmapToBitmapImage(Bitmap bitmap) //Omvandlar en bitmap till en bitmapimage
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
        private void LoadBitmaptoCanvas(Bitmap bitmap)
        {
            System.Windows.Media.ImageBrush ib = new();
            ib.ImageSource = BitmapToBitmapImage(bitmap);

            StandardCanvas.Background = ib;
        }

        private void FloodFill(Bitmap bmp, System.Windows.Point pt, Color replacementColor, Color targetColor) //Fyller en friformat ritat figur
        {
            targetColor = bmp.GetPixel((int)pt.X, (int)pt.Y);
            if (targetColor.ToArgb().Equals(replacementColor.ToArgb()))
            {
                return;
            }

            Stack<System.Windows.Point> pixels = new Stack<System.Windows.Point>();

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

                    if (temp.X > 1 && !spanLeft && temp.X > 0 && bmp.GetPixel((int)temp.X - 1, y1) == targetColor) //John gjorde koden som är dålig och fungerar inte @admin banna Johnmaster64 genast
                    {
                        pixels.Push(new System.Windows.Point(temp.X - 1, y1));
                        spanLeft = true;
                    }
                    else if (spanLeft && temp.X - 1 == 0 && bmp.GetPixel((int)temp.X - 1, y1) != targetColor)
                    {
                        spanLeft = false;
                    }
                    if (!spanRight && temp.X < bmp.Width - 1 && bmp.GetPixel((int)temp.X + 1, y1) == targetColor)
                    {
                        pixels.Push(new System.Windows.Point(temp.X + 1, y1));
                        spanRight = true;
                    }
                    else if (spanRight && temp.X < bmp.Width - 1 && bmp.GetPixel((int)temp.X + 1, y1) != targetColor)
                    {
                        spanRight = false;
                    }
                    y1++;
                }

            }
            LoadBitmaptoCanvas(bmp);
        }
    }
}
