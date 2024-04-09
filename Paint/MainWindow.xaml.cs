﻿using System.Collections.Generic;
using System.Diagnostics.SymbolStore;
using System.Windows;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using System.IO;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Media.Imaging;
using ColorPickerControls.Pickers;
using System.Windows.Controls;
using System.Security.Policy;

namespace Paint
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private System.Windows.Media.Color _color;
        private Tools _tools;
        StateHandler _stateHandler;

        private bool handle = true;
        System.Windows.Point start; 
        System.Windows.Point end;
        

        public MainWindow()
        {
            InitializeComponent();

            _tools = new(StandardCanvas);
            _stateHandler = new(StandardCanvas);

            //anger standard drawing tool som pentool och standard färgen som svart
            _tools.SetTool("pen");
            SizeSlider.Maximum = 25;
            SizeSlider.Minimum = 1;

            
            _color = Colors.DarkOrchid;  // sätter standard färgen
            _tools.SetColor(_color); // ger det valda verktyget standard färgen 
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
                if (StandardCanvas.DefaultDrawingAttributes == _tools.GetTool("eraser") || StandardCanvas.DefaultDrawingAttributes == _tools.GetTool("pen"))
                {
                    _stateHandler.AddToAdded(e.Added); // ritade sträck läggs till här
                    _stateHandler.ClearRemove(); // gör så att man inte kan redo:a sträck som togs bort innan det senaste sträcket
                }
                else if (StandardCanvas.DefaultDrawingAttributes == _tools.GetTool("line") || StandardCanvas.DefaultDrawingAttributes == _tools.GetTool("rectangle"))
                {
                    _stateHandler.AddToVisualStrokes(e.Added); // ritade shapes läggs till här
                    _stateHandler.ClearRemove();
                }
            }   
        }

        /* Buttons */
        private void EraserToolButton(object sender, RoutedEventArgs e)
        {
            _tools.SetTool("eraser");
            UpdateSizeSlider();
        }

        private void PencilToolButton(object sender, RoutedEventArgs e)
        {
            _tools.SetTool("pen");
            UpdateSizeSlider();
        }

        private void LineToolButton(object sender, RoutedEventArgs e)
        {
            _tools.SetTool("line");
            UpdateSizeSlider();

        }
        private void RectangleToolButton(object sender, RoutedEventArgs e)
        {
            _tools.SetTool("rectangle");
            UpdateSizeSlider();
        }

        private void EllipseToolButton(object sender, RoutedEventArgs e)
        {
            _tools.SetTool("ellipse");
            UpdateSizeSlider();
        }

        public void FillToolButton(object sender, RoutedEventArgs e)
        {
            _tools.SetTool("fill");
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
            _tools.LoadImageFromFile();
        }

        private void ClearButtonClick(object sender, RoutedEventArgs e)
        {
            ClearStrokes();
            ClearBackground();
        }
        /* Buttons */

        private void ClearBackground()
        {
            SolidColorBrush solidColorBrush = new SolidColorBrush();
            solidColorBrush.Color = Colors.White;

            StandardCanvas.Background = solidColorBrush;
        }

        private void ClearStrokes()
        {
            StandardCanvas.Strokes.Clear();
            _stateHandler.ClearAdded();
            DrawFirstLine();
        }

        public void DrawFirstLine()  
        {
            Stroke newStroke = _tools._drawTools.DrawLine(new System.Windows.Point(0, 0), new System.Windows.Point(0, 0), Colors.Transparent, SizeSlider.Value);
            _stateHandler.AddToAdded(new StrokeCollection() { newStroke });   
        }

        
        private void StandardCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //kollar vilket verktyg som är aktivt
            if (StandardCanvas.DefaultDrawingAttributes == _tools.GetTool("line") || StandardCanvas.DefaultDrawingAttributes == _tools.GetTool("rectangle") || StandardCanvas.DefaultDrawingAttributes == _tools.GetTool("ellipse"))
            {
                start = e.GetPosition(StandardCanvas); // ger formen sin start koordinat
            }
            else if (StandardCanvas.DefaultDrawingAttributes == _tools.GetTool("fill"))
            {
                System.Drawing.Color targetColor = GetColorUnderMouse(e);
                System.Drawing.Color replacementColor = System.Drawing.Color.FromArgb(_color.A, _color.R, _color.G, _color.B);

                if (targetColor != replacementColor)
                {
                    StylusPointCollection pts = _tools._drawTools.FloodFill(_tools.RenderTargetBitmap(), e.GetPosition(StandardCanvas), targetColor, replacementColor);
                    DrawingAttributes dA = new() { Color = _color, Height = SizeSlider.Value, Width = SizeSlider.Value, StylusTip = StylusTip.Rectangle };
                    StrokeCollection newStroke = new StrokeCollection() { new Stroke(pts, dA) };

                    _stateHandler.AddToCanvas(newStroke);
                    _stateHandler.AddToAdded(newStroke);
                }   
            }
        }

        private void StandardCanvas_MouseUp(object sender, MouseButtonEventArgs e)  // 
        {
            if (StandardCanvas.DefaultDrawingAttributes == _tools.GetTool("line") || StandardCanvas.DefaultDrawingAttributes == _tools.GetTool("rectangle") || StandardCanvas.DefaultDrawingAttributes == _tools.GetTool("ellipse"))
            {
                _stateHandler.AddVisualLineToCanvas();
            }
        }

        private void StandardCanvas_MouseMove(object sender, MouseEventArgs e) // trackar hela tiden när musen rör sig vart den är
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                end = e.GetPosition(StandardCanvas);

                if (StandardCanvas.DefaultDrawingAttributes == _tools.GetTool("rectangle") || StandardCanvas.DefaultDrawingAttributes == _tools.GetTool("line") || StandardCanvas.DefaultDrawingAttributes == _tools.GetTool("ellipse"))
                {
                    _stateHandler.RemoveVisualLine();
                    Stroke newStroke = _tools._drawTools.DrawLine(new System.Windows.Point(0, 0), new System.Windows.Point(0, 0), Colors.Transparent, SizeSlider.Value); // skapar en default stroke som sedan kan ändras

                    if (StandardCanvas.DefaultDrawingAttributes == _tools.GetTool("line"))
                    {
                        newStroke = _tools._drawTools.DrawLine(start, end, _color, SizeSlider.Value); ; // skapar newLine
                    }
                    else if (StandardCanvas.DefaultDrawingAttributes == _tools.GetTool("rectangle"))
                    {
                        newStroke = _tools._drawTools.DrawRectangle(start, end, _color, SizeSlider.Value);
                    }
                    else if (StandardCanvas.DefaultDrawingAttributes == _tools.GetTool("ellipse"))
                    {
                        newStroke = _tools._drawTools.DrawEllipse(start, end, 500, _color, SizeSlider.Value);
                    }
                    StrokeCollection strokeCollection = new() { newStroke };  //Lägger newLine i strokecollection

                    _stateHandler.AddToVisualStrokes(strokeCollection);
                    StandardCanvas.Strokes.Add(strokeCollection);
                }
            }
        }

        private void PaintKeyDown(object sender,  KeyEventArgs e)
        {
            if (e.Key == Key.Z)
                _stateHandler.Undo();
            else if (e.Key == Key.Y)
                _stateHandler.Redo();
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

        private void UpdateWindow(object sender, RoutedEventArgs e) //Uppdaterar storleken av canvas:en när storleken av programmet ändras för att reflektera ändringen
        {
            StandardCanvas.Height = (Paint.Height / 3) * 2;
            StandardCanvas.Width = (Paint.Width / 3) * 2;
        }

        private void UpdateWindowMax(object sender, RoutedEventArgs e)
        {
            StandardCanvas.Height = (Paint.Height / 3) * 2;
            StandardCanvas.Width = (Paint.Width / 3) * 2;
        }

        private System.Drawing.Color GetColorUnderMouse(MouseEventArgs m) //return:ar färgen där man klickar
        {
            return _tools.RenderTargetBitmap().GetPixel((int)m.GetPosition(StandardCanvas).X, (int)m.GetPosition(StandardCanvas).Y);
        }
    }
}
