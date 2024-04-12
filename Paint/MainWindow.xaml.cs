using System.Windows;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Controls;
using System.Text.RegularExpressions;


namespace Paint
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Tools _tools;
        StateHandler _stateHandler;

        bool handle = true;
        Point _start; 
        Point _end;
        Color _color;

        public MainWindow()
        {
            InitializeComponent();

            _tools = new(StandardCanvas);
            _stateHandler = new(StandardCanvas);

            //Defines the starting tool as pen and gives it the standard color as black
            _tools.SetTool("pen");
            _color = Colors.Black;
            ColorChanged(); 

            StandardCanvas.UseCustomCursor = true;

            //Defines the sizeSliders Min and Max values
            SizeSlider.Maximum = 25;
            SizeSlider.Minimum = 1;

            DrawFirstLine();
            StandardCanvas.Strokes.StrokesChanged += Strokes_StrokesChanged;
        }


        private void Strokes_StrokesChanged(object sender, StrokeCollectionChangedEventArgs e)
        {
            if (handle) //Makes sure that no lines can be added while undo/redo is used
            {
                if (_tools.GetTool() == "eraser" || _tools.GetTool() == "pen")
                {
                    _stateHandler.AddToAdded(e.Added); //Drawn strokes gets added here
                    _stateHandler.ClearRemove(); //Prevents the user from redoing strokes that was remove before the latest stroke was added
                }
                else if (_tools.GetTool() == "line" || _tools.GetTool() == "rectangle" || _tools.GetTool() == "ellipse")
                {
                    _stateHandler.AddToVisualStrokes(e.Added); //Drawn shapes gets added here
                    _stateHandler.ClearRemove();
                }
            }   
        }

        /* Buttons */
        private void PencilToolButton(object sender, RoutedEventArgs e)
        {
            _tools.SetTool("pen");
            UpdateSizeSlider();

            ResetAllButtons();
            PencilTool.Background = new SolidColorBrush(Colors.Gray);
        }

        private void EraserToolButton(object sender, RoutedEventArgs e)
        {
            _tools.SetTool("eraser");
            UpdateSizeSlider();

            ResetAllButtons();
            EraserTool.Background = new SolidColorBrush(Colors.Gray);
        }

        private void ColorPickerButton(object sender, RoutedEventArgs e)
        {
            _tools.SetTool("colorPicker");

            ResetAllButtons();
            ColorPickerTool.Background = new SolidColorBrush(Colors.Gray);
        }

        private void LineToolButton(object sender, RoutedEventArgs e)
        {
            _tools.SetTool("line");
            UpdateSizeSlider();

            ResetAllButtons();
            LineTool.Background = new SolidColorBrush(Colors.Gray);
        }
        private void RectangleToolButton(object sender, RoutedEventArgs e)
        {
            _tools.SetTool("rectangle");
            UpdateSizeSlider();

            ResetAllButtons();
            RectangleTool.Background = new SolidColorBrush(Colors.Gray);
        }

        private void EllipseToolButton(object sender, RoutedEventArgs e)
        {
            _tools.SetTool("ellipse");
            UpdateSizeSlider();

            ResetAllButtons();
            EllipseTool.Background = new SolidColorBrush(Colors.Gray);
        }

        public void FillToolButton(object sender, RoutedEventArgs e)
        {
            _tools.SetTool("fill");

            ResetAllButtons();
            FillTool.Background = new SolidColorBrush(Colors.Gray);
        }

        private void SaveButtonClick(object sender, RoutedEventArgs e)
        {
            _tools.SaveCanvas();
        }

        private void RedoButton(object sender, RoutedEventArgs e)
        {
            handle = false;
            _stateHandler.Redo();
            handle = true;
        }

        private void UndoButton(object sender, RoutedEventArgs e)
        {
            handle = false;
            _stateHandler.Undo();
            handle = true;
        }

        private void LoadButtonClick(object sender, RoutedEventArgs e)
        {
            if (!_tools.LoadImageFromFile())
                ErrorText.Content = "Error: Load file failed, did you select a file?";
            ClearStrokes();
        }

        private void ClearButtonClick(object sender, RoutedEventArgs e)
        {
            ClearStrokes();
            ClearBackground();


            //Used for user testing:
            //string url = "https://forms.gle/Lc9Fg9uEeutcVWnYA";
            //Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
        }

        private void ResetAllButtons() //resets all tool buttons to the same lightgray color, allowing the choosen color to be changed to visualize which tool is currently selected
        {
            PencilTool.Background = new SolidColorBrush(Colors.LightGray);
            EraserTool.Background = new SolidColorBrush(Colors.LightGray);
            ColorPickerTool.Background = new SolidColorBrush(Colors.LightGray);
            FillTool.Background = new SolidColorBrush(Colors.LightGray);
            LineTool.Background = new SolidColorBrush(Colors.LightGray);
            EllipseTool.Background = new SolidColorBrush(Colors.LightGray);
            RectangleTool.Background = new SolidColorBrush(Colors.LightGray);
        }
        /* Buttons */

        /* HID inputs*/
        private void StandardCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            ErrorText.Content = "";

            //Checks which tool is activated
            if (_tools.GetTool() == "line" || _tools.GetTool() == "rectangle" || _tools.GetTool() == "ellipse")
            {
                _start = e.GetPosition(StandardCanvas); //saves starting coordinate if one of the tools which needs it is used
            }
            else if (_tools.GetTool() == "fill")
            {
                System.Drawing.Color targetColor = GetColorUnderMouse(e); //Saves which color to replace during fill

                
                System.Drawing.Color replacementColor = System.Drawing.Color.FromArgb(_color.A, _color.R, _color.G, _color.B); 

                if (targetColor != replacementColor)
                {
                    StrokeCollection newStroke = _tools._drawTools.FloodFill(_tools.RenderTargetBitmap(), e.GetPosition(StandardCanvas), targetColor, replacementColor, SizeSlider.Value);

                    _stateHandler.AddToCanvas(newStroke);
                    _stateHandler.AddToAdded(newStroke);
                }   
            }
            else if (_tools.GetTool() == "colorPicker") 
            { 
                System.Drawing.Color color = GetColorUnderMouse(e); 

                _color = Color.FromArgb(color.A, color.R, color.G, color.B);
                ColorChanged();

                PencilToolButton(null, null); //enables pencilTool as it is most commonly used after picking a color (in the future we could make it so that it changes back to the previous tool)
            }
        }

        private void StandardCanvas_MouseUp(object sender, MouseButtonEventArgs e)   
        {
            if (_tools.GetTool() == "line" || _tools.GetTool() == "rectangle" || _tools.GetTool() == "ellipse")
            {
                _stateHandler.AddVisualLineToCanvas(); //the latest visual line gets added to the canvas as a permanent stroke
            }
        }

        private void StandardCanvas_MouseMove(object sender, MouseEventArgs e) //tracks when the mouse moves as to add "visual lines" 
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                _end = e.GetPosition(StandardCanvas);

                if (_tools.GetTool() == "rectangle" || _tools.GetTool() == "line" || _tools.GetTool() == "ellipse")
                {
                    _stateHandler.RemoveVisualLine(); //Removes the current visual line before adding a new one

                    StrokeCollection newStroke = new();

                    if (_tools.GetTool() =="line")
                    {
                        newStroke = _tools._drawTools.DrawLine(_start, _end, _color, SizeSlider.Value); ; // skapar newLine
                    }
                    else if (_tools.GetTool() == "rectangle")
                    {
                        newStroke = _tools._drawTools.DrawRectangle(_start, _end, _color, SizeSlider.Value);
                    }
                    else if (_tools.GetTool() == "ellipse")
                    {
                        newStroke = _tools._drawTools.DrawEllipse(_start, _end, 500, _color, SizeSlider.Value);
                    }

                    _stateHandler.AddToVisualStrokes(newStroke);
                    StandardCanvas.Strokes.Add(newStroke);
                }
            }
        }

        private void PaintKeyDown(object sender,  KeyEventArgs e) //calls undo when Z is pressed and redo when Y is pressed
        {
            if (e.Key == Key.Z)
            {
                handle = false;
                _stateHandler.Undo();
                handle = true;
            }
            else if (e.Key == Key.Y)
            {
                handle = false;
                _stateHandler.Redo();
                handle = true;
            }
        }
        /* HID inputs*/

        private void SizeSliderValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) //Gives the SizeSliderValue to the current tools height and width, also changes the Visul size ellipse the new sizeSlider value
        {
            StandardCanvas.DefaultDrawingAttributes.Height = SizeSlider.Value; 
            StandardCanvas.DefaultDrawingAttributes.Width = SizeSlider.Value;

            VisualSize.Width = SizeSlider.Value;
            VisualSize.Height = SizeSlider.Value;
        }

        private void UpdateSizeSlider() //Changes the SizeSlider Value to represent the choosen tools size value
        {
            SizeSlider.Value = StandardCanvas.DefaultDrawingAttributes.Width;
        }

        private void ClearBackground() //Clears the background by chaging it to a solidColorBrush with the color white
        {
            SolidColorBrush solidColorBrush = new SolidColorBrush();
            solidColorBrush.Color = Colors.White;

            StandardCanvas.Background = solidColorBrush;
        }

        private void ClearStrokes() //Clears all strokes from the canvas
        {
            StandardCanvas.Strokes.Clear();
            _stateHandler.ClearAdded();
            DrawFirstLine();
        }

        public void DrawFirstLine() //Draw an invisible line that is needed for future refernce as _added can never be empty
        {
            StrokeCollection newStroke = _tools._drawTools.DrawLine(new Point(0, 0), new Point(0, 0), Colors.Transparent, 1);
            _stateHandler.AddToAdded(newStroke);
        }

        private System.Drawing.Color GetColorUnderMouse(MouseEventArgs m) //Returns the color value at the point where the mouse is
        {
            return _tools.RenderTargetBitmap().GetPixel((int)m.GetPosition(StandardCanvas).X, (int)m.GetPosition(StandardCanvas).Y);
        }

        private void UpdateSliders() //updates the colorSliders to represent the current _color
        {
            if (redSlider != null && greenSlider != null && blueSlider != null)
            {
                redSlider.Value = _color.R;
                greenSlider.Value = _color.G;
                blueSlider.Value = _color.B;
            }
        }

        private void UpdateTextboxes() //updates the colorTextBoxes to represent the current _color
        {
            if (redTextBox != null && redTextBox.Text != "" && greenTextBox != null && greenTextBox.Text != "" && blueTextBox != null && blueTextBox.Text != "")
            {
                redTextBox.Text = _color.R.ToString();
                greenTextBox.Text = _color.G.ToString();
                blueTextBox.Text = _color.B.ToString();
            }
        }

        private void UpdateVisualizers() //updates both the color and the size visualizer to represent the current _color
        {
            if (VisualSize != null && VisualColor != null)
            {
                SolidColorBrush brush = new(_color);
                VisualColor.Fill = brush;
                VisualSize.Fill = brush;
            }
        }


        private void CheckIfint(object sender, TextCompositionEventArgs e) //Checks if the textBox's input was an int, if it was not then it will not be added to the textBox
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void ColorChanged() //When _color is changed this function gives the new color values to all visualizers such as the sliders and textBoxes 
        {
            handle = false; //handle is used here to prevent the Sliders and textboxes' update functions to run when running this function, UpdateColorSlider() would be called when the sliders value was changed, resulting in only one color value being changed at a time
            UpdateSliders();
            UpdateTextboxes();
            UpdateVisualizers();
            

            if (_tools != null)
                _tools.SetColor(_color);
            handle = true;
        }

        private void UpdateColorSlider(object sender, RoutedPropertyChangedEventArgs<double> e) //Gives _color the new color values when an input is made with the slider 
        {
            if (handle)
            {
                byte red = 0;
                byte green = 0;
                byte blue = 0;

                if (redSlider != null && greenSlider != null && redSlider != null)
                {
                    red = (byte)redSlider.Value;
                    green = (byte)greenSlider.Value;
                    blue = (byte)blueSlider.Value;
                }

                _color = Color.FromRgb(red, green, blue);

                ColorChanged();
            }
        }

        private void UpdateColorTextbox(object sender, TextChangedEventArgs e) //Gives _color the new color values when an input is made with the TextBoxes
        {
            if (handle)
            {
                byte red = 0;
                byte green = 0;
                byte blue = 0;

                if (redTextBox != null && redTextBox.Text != "" && greenTextBox != null && greenTextBox.Text != "" && blueTextBox != null && blueTextBox.Text != "")
                {
                    if (int.Parse(redTextBox.Text) > 255) //A byte can not contain more than 255 so if the input number is bigger than this, it gets defaultet to 255
                        red = 255;
                    else
                        red = byte.Parse(redTextBox.Text);

                    if (int.Parse(greenTextBox.Text) > 255)
                        green = 255;
                    else
                        green = byte.Parse(greenTextBox.Text);

                    if (int.Parse(blueTextBox.Text) > 255)
                        blue = 255;
                    else
                        blue = byte.Parse(blueTextBox.Text);
                }

                _color = Color.FromRgb(red, green, blue);

                ColorChanged();
            }
        }
    }
}
