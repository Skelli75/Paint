using System.Collections.Generic;
using System.Diagnostics.SymbolStore;
using System.Windows;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

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
        private Color _color;
        private ShapeTools _shapeTools;

        private bool handle = true;
        Point start;  // skapar start och end punkterna
        Point end;

        private readonly DrawingAttributes PenTool = new() // ha vriabler för storlek
        {
            Height = 2,
            Width = 2
        };

        private readonly DrawingAttributes EraserTool = new() // byt white till chosen
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
        public MainWindow()
        {
            InitializeComponent();
            StandardCanvas.UseCustomCursor = true;
            StandardCanvas.DefaultDrawingAttributes = PenTool;
            SizeSlider.Maximum = 25;
            SizeSlider.Minimum = 1;

            _shapeTools = new ShapeTools();
            _color = Colors.DarkOrchid;  // sätter standard färgen
            PenTool.Color = _color; // ger pennan standard färgen för det gick inte i konstruktorn

            DrawFirstLine();
            StandardCanvas.Strokes.StrokesChanged += Strokes_StrokesChanged;
        }

        private void Strokes_StrokesChanged(object sender, System.Windows.Ink.StrokeCollectionChangedEventArgs e)
        {
            if (handle)
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

        private void Undo()
        {
            handle = false;
            if (_added.Count > 1)
            {
                _removed.Add(_added[_added.Count - 1]); // sparar borttaget sträck
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
                _added.Add(_removed[_removed.Count - 1]); // lägger till sträcken i "finns" listan
                StandardCanvas.Strokes.Add(_removed[_removed.Count - 1]); // lägger till sträcket på tavlan
                _removed.Remove(_removed[_removed.Count - 1]); // tar bort sträcket från bortaget listan
            }
            handle = true;
        }

        private void ButtonEraserTool(object sender, RoutedEventArgs e)
        {
            StandardCanvas.DefaultDrawingAttributes = EraserTool;
            UpdateSizeSlider();
        }

        private void ButtonPencilTool(object sender, RoutedEventArgs e)
        {
            StandardCanvas.DefaultDrawingAttributes = PenTool;
            UpdateSizeSlider();
        }

        private void SizeSliderValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            StandardCanvas.DefaultDrawingAttributes.Height = SizeSlider.Value;
            StandardCanvas.DefaultDrawingAttributes.Width = SizeSlider.Value;
        }

        private void UpdateSizeSlider()
        {
            SizeSlider.Value = StandardCanvas.DefaultDrawingAttributes.Width;
        }

        private void RedoButton(object sender, RoutedEventArgs e)
        {
            Redo();
        }

        private void UndoButton(object sender, RoutedEventArgs e)
        {
            Undo();
        }
        private void SaveButtonClick(object sender, RoutedEventArgs e)
        {

        }

        private void LoadButtonClick(object sender, RoutedEventArgs e)
        {

        }

        public void DrawFirstLine()  
        {

            Stroke newStroke = _shapeTools.DrawLine(new Point(0,0), new Point(0,0), Colors.Transparent, SizeSlider.Value);

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
            if (StandardCanvas.DefaultDrawingAttributes == LineTool || StandardCanvas.DefaultDrawingAttributes == RectangleTool)
            {
                start = e.GetPosition(StandardCanvas); // ger formen sin start koordinat
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
                    Stroke newStroke = _shapeTools.DrawLine(new Point(0, 0), new Point(0, 0), Colors.Transparent, SizeSlider.Value); // ritar ut en linje som sedan ska ändras
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

        private void ButtonFillTool(object sender, RoutedEventArgs e)
        {

        }

        private void ButtonLineTool(object sender, RoutedEventArgs e)  // vilken from som ska göras
        {
            StandardCanvas.DefaultDrawingAttributes = LineTool; // stänger av pennan 
        }
        private void ButtonRectangleTool(object sender, RoutedEventArgs e)
        {
            StandardCanvas.DefaultDrawingAttributes = RectangleTool;  
        }

        private void ButtonElipseTool(object sender, RoutedEventArgs e)
        {

        }
        private void ButtonBentLineTool(object sender, RoutedEventArgs e)
        {

        }
    }
}
