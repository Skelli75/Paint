using System;
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
        }

        private void ButtonEraserTool(object sender, RoutedEventArgs e)
        {
            StandardCanvas.DefaultDrawingAttributes = EraserTool;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {

        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            PenTool.Height = SizeSlider.Value;
            PenTool.Width = SizeSlider.Value;
        }
    }
}
