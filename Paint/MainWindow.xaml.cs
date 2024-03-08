using System.Collections.Generic;
using System.Diagnostics.SymbolStore;
using System.Windows;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
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
        List<System.Windows.Ink.StrokeCollection> _added = new(); // två listor, en med sträck som finns och en med borttagna
        List<System.Windows.Ink.StrokeCollection> _visualStrokes = new(); // två listor, en med sträck som finns och en med borttagna
        List<System.Windows.Ink.StrokeCollection> _removed = new();
        private System.Windows.Media.Color _color;
        private ShapeTools _shapeTools;

        private bool handle = true;
        System.Windows.Point start;  // skapar start och end punkterna
        System.Windows.Point end;
        
        /* Definerar drawing tools */
        private readonly DrawingAttributes PenTool = new() 
        {
            Color = Colors.DarkGreen,
            Height = 2,
            Width = 2
        };

        private readonly DrawingAttributes EraserTool = new() 
        {
            Color = Colors.White,
            Height = 4,
            Width = 4
        };

        private readonly DrawingAttributes LineTool = new() // ha vriabler för storlek
        {
            Color = Colors.Transparent,
            Width = 1,
        };

        private readonly DrawingAttributes RectangleTool = new() // ha vriabler för storlek
        {
            Color = Colors.Transparent,
            Width = 2,  // endast för att skilja den frå Linetool då de annars förvirras med varandra av koden
        };

        private readonly DrawingAttributes FillTool = new()
        {
            Color = Colors.Transparent,
        };
        /* Definerar drawing tools */

        public MainWindow()
        {
            InitializeComponent();

            //anger standard drawing tool som pentool och standard färgen som svart
            StandardCanvas.DefaultDrawingAttributes = PenTool;
            SizeSlider.Maximum = 25;
            SizeSlider.Minimum = 1;

            _shapeTools = new ShapeTools();
            _color = Colors.DarkOrchid;  // sätter standard färgen
            PenTool.Color = _color; // ger pennan standard färgen för det gick inte i konstruktorn
            StandardCanvas.UseCustomCursor = true;

            //Ger sizeslider min och max värden
            SizeSlider.Maximum = 100;
            SizeSlider.Minimum = 4;

            DrawFirstLine();
            StandardCanvas.Strokes.StrokesChanged += Strokes_StrokesChanged;
        }

        private void Strokes_StrokesChanged(object sender, StrokeCollectionChangedEventArgs e)
        {
            if (handle) //ser till så att inga sträck kan läggas till medans undo och redo utförs
            {
                if (StandardCanvas.DefaultDrawingAttributes == EraserTool || StandardCanvas.DefaultDrawingAttributes == PenTool)
                {
                    _added.Add(e.Added); // ritade sträck lägg läggs till här
                    _removed.Clear(); // gör så att man inte kan redo:a sträck som togs bort innan det senaste sträcket
                }
                else if (StandardCanvas.DefaultDrawingAttributes == LineTool || StandardCanvas.DefaultDrawingAttributes == RectangleTool)
                {
                    _visualStrokes.Add(e.Added); // ritade sträck lägg läggs till här
                    _removed.Clear();
                }
            }   
        }

        private void ClearStrokes()
        {
            StandardCanvas.Strokes.Clear();
            _added.Clear();
            DrawFirstLine();
        }

        private void Undo()
        {
            handle = false;
            if (_added.Count > 1)
            {
                _removed.Add(_added[_added.Count - 1]); // sparar det senaste ritade sträcket för att senare kunna redo:a om det önskas
                StandardCanvas.Strokes.Remove(_added[_added.Count - 1]); // tar bort det senaste ritade sträcket från tavlan
                _added.Remove(_added[_added.Count - 1]); // tar bort sträcket från "finns på tavlan" listan
            }
            handle = true;
        }

        private void Redo()
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

        private void SaveButtonClick(object sender, RoutedEventArgs e)
        {
            SaveCanvas();
        }

        private void RedoButton(object sender, RoutedEventArgs e)
        {
            Redo();
        }

        private void UndoButton(object sender, RoutedEventArgs e)
        {
            Undo();
        }

        private void LineToolButton(object sender, RoutedEventArgs e)  // vilken from som ska göras
        {
            StandardCanvas.DefaultDrawingAttributes = LineTool; // stänger av pennan 
        }
        private void RectangleToolButton(object sender, RoutedEventArgs e)
        {
            StandardCanvas.DefaultDrawingAttributes = RectangleTool;
        }

        private void EllipseToolButton(object sender, RoutedEventArgs e)
        {

        }

        private void LoadButtonClick(object sender, RoutedEventArgs e)
        {
            LoadImageFromFile();
        }

        private void ClearButtonClick(object sender, RoutedEventArgs e)
        {
            ClearStrokes();
        }
        /* Buttons */


        public void DrawFirstLine()  
        {

            Stroke newStroke = _shapeTools.DrawLine(new System.Windows.Point(0,0), new System.Windows.Point(0,0), Colors.Transparent, SizeSlider.Value);

            StrokeCollection strokeCollection = new();
            strokeCollection.Add(newStroke);

            _added.Add(strokeCollection);
            _visualStrokes.Add(strokeCollection);
            StandardCanvas.Strokes.Add(newStroke); // lägger till linjen på tavlan

        }

        private void RemoveVisualLine()
        {
            if (_added.Count > 0 && _visualStrokes.Count > 0)
            {
                if (_added[_added.Count - 1] != _visualStrokes[_visualStrokes.Count - 1]) // är det senaste sträcket ett permanent sträck?
                {                                               // nej
                    StandardCanvas.Strokes.Remove(_visualStrokes[_visualStrokes.Count - 1]); // ta då bort det
                    //_visualStroke
                }
            }
        }
        private void StandardCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //kollar vilket verktyg som är aktivt
            if (StandardCanvas.DefaultDrawingAttributes == LineTool || StandardCanvas.DefaultDrawingAttributes == RectangleTool)
            {
                start = e.GetPosition(StandardCanvas); // ger formen sin start koordinat
            }
            else if (StandardCanvas.DefaultDrawingAttributes == FillTool)
            {
                System.Drawing.Color targetColor = GetColorUnderMouse(e);
                System.Drawing.Color replacementColor = System.Drawing.Color.FromArgb(_color.A, _color.R, _color.G, _color.B);

                if (targetColor != replacementColor)
                {
                    FloodFill(RenderTargetBitmap(), e.GetPosition(StandardCanvas), replacementColor, targetColor);
                }
            }
        }
        private void StandardCanvas_MouseUp(object sender, MouseButtonEventArgs e)  // aktiverar
        {
            if (StandardCanvas.DefaultDrawingAttributes == LineTool || StandardCanvas.DefaultDrawingAttributes == RectangleTool)
            {
                StrokeCollection strokecollection = new();
                strokecollection.Add(_visualStrokes[_visualStrokes.Count - 1]); // lägger in den senaste visual linen i den riktiga line listan
                _added.Add(strokecollection);
                _visualStrokes.Clear(); // tömmer listan
            }
        }

        private void StandardCanvas_MouseMove(object sender, MouseEventArgs e) // trackar hela tiden när musen rör sig vart den är
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (StandardCanvas.DefaultDrawingAttributes == RectangleTool || StandardCanvas.DefaultDrawingAttributes == LineTool) 
                {

                    end = e.GetPosition(StandardCanvas);                                                  // tar koordinaten av slutet av linjen/formen
                    RemoveVisualLine();                                                                 // tar bort förra utritade visualline
                    Stroke newStroke = _shapeTools.DrawLine(new System.Windows.Point(0, 0), new System.Windows.Point(0, 0), Colors.Transparent, SizeSlider.Value); // ritar ut en linje som sedan ska ändras
                    StrokeCollection strokecollection = new();  //Lägger newLine i strokecollection

                    if (StandardCanvas.DefaultDrawingAttributes == LineTool)
                    {
                        newStroke = _shapeTools.DrawLine(start, end, _color, SizeSlider.Value); ; // skapar newLine
                    }
                    else if (StandardCanvas.DefaultDrawingAttributes == RectangleTool)
                    {
                        newStroke = _shapeTools.DrawRectangle(start, end, _color, SizeSlider.Value);
                    }

                    strokecollection.Add(newStroke);
                    _visualStrokes.Add(strokecollection);

                    StandardCanvas.Strokes.Add(newStroke); // lägger till newLine på tavlan
                }
            }
        }

        

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

        private System.Drawing.Color GetColorUnderMouse(MouseEventArgs m) //return:ar färgen där man klickar
        {
            return RenderTargetBitmap().GetPixel((int)m.GetPosition(StandardCanvas).X, (int)m.GetPosition(StandardCanvas).Y);
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
        private void LoadBitmaptoCanvas(Bitmap bitmap) //Laddar in en bitmap på canvas:en
        {
            System.Windows.Media.ImageBrush ib = new();
            ib.ImageSource = BitmapToBitmapImage(bitmap);

            StandardCanvas.Background = ib;
        }

        private void FloodFill(Bitmap bmp, System.Windows.Point pt, System.Drawing.Color replacementColor, System.Drawing.Color targetColor) //Fyller en friformat ritat figur
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
            ClearStrokes();
            LoadBitmaptoCanvas(bmp);
        }
    }
}
