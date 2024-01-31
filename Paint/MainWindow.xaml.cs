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
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Ink;

namespace Paint
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        List<System.Windows.Ink.StrokeCollection> _added = new(); // två listor, en med sträck som finns och en med borttagna
        List<System.Windows.Ink.StrokeCollection> _removed = new();
        private bool handle = true;

        private readonly DrawingAttributes PenTool = new() // ha vriabler för storlek
        {
            Color = Colors.Black,
            Height = 2,
            Width = 2
        };

        private readonly DrawingAttributes EraserTool = new () // byt white till chosen
        {
            Color = Colors.White,
            Height = 4,
            Width = 4
        };
        public MainWindow()
        {
            InitializeComponent();
            StandardCanvas.UseCustomCursor = true;
            StandardCanvas.DefaultDrawingAttributes = PenTool;
            SizeSlider.Maximum = 100;
            SizeSlider.Minimum = 4;

            StandardCanvas.Strokes.StrokesChanged += Strokes_StrokesChanged;
        }

        private void Strokes_StrokesChanged(object sender, System.Windows.Ink.StrokeCollectionChangedEventArgs e)
        {
            if (handle)
            {
                _added.Add(e.Added); // ritade sträck lägg läggs till här
                _removed.Clear(); // gör så att man inte kan redo:a sträck som togs bort innan det senaste sträcket
            }
        }

        private void Undo(object sender, RoutedEventArgs e)
        {
            handle = false;
            if (_added.Count > 0)
            {
                _removed.Add(_added[_added.Count - 1]); // sparar borttaget sträck
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
                _added.Add(_removed[_removed.Count-1]); // lägger till sträcken i "finns" listan
                StandardCanvas.Strokes.Add(_removed[_removed.Count-1]); // lägger till sträcket på tavlan
                _removed.Remove(_removed[_removed.Count-1]); // tar bort sträcket från bortaget listan
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
            Redo(sender, e);
        }

        private void UndoButton(object sender, RoutedEventArgs e)
        {
            Undo(sender, e);
        }
    }
}
