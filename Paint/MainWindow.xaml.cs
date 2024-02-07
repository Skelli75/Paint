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
using Microsoft.Win32;
using System.IO;

namespace Paint
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly DrawingAttributes PenTool = new() // ha vriabler för storlek
        {
            Color = Colors.HotPink,
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
            StandardCanvas.Height = mainWindow.Height / 2;
            StandardCanvas.Width = mainWindow.Width / 2;
            StandardCanvas.Margin = new Thickness((mainWindow.Width / 2) - (StandardCanvas.Width / 2), (mainWindow.Height / 2) - (StandardCanvas.Height / 2), (mainWindow.Width / 2) - (StandardCanvas.Width / 2), (mainWindow.Height / 2) - (StandardCanvas.Height / 2));
            SizeSlider.Maximum = 100;
            SizeSlider.Minimum = 4;
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

        private void SaveButtonClick(object sender, RoutedEventArgs e)
        {
            SaveFileDialog save = new();
            save.Filter = "JPeg Image|*.jpg";
            save.Title = "Save an Image File";
            save.FileName = "default";
            save.ShowDialog();

            StandardCanvas.Margin = new Thickness(0, 0, 0, 0);
            StandardCanvas.Arrange(new Rect(new Size(StandardCanvas.Width, StandardCanvas.Height)));

            RenderTargetBitmap renderBitmap = new((int)StandardCanvas.Width, (int)StandardCanvas.Height, 96, 96, PixelFormats.Default);
            renderBitmap.Render(StandardCanvas);   

            FileStream fs = File.Open(save.FileName, FileMode.OpenOrCreate);
            JpegBitmapEncoder encoder = new JpegBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(renderBitmap));
            encoder.Save(fs);

            StandardCanvas.Margin = new Thickness((mainWindow.Width/2) - (StandardCanvas.Width/2), (mainWindow.Height/2) - (StandardCanvas.Height/2), (mainWindow.Width / 2) - (StandardCanvas.Width / 2), (mainWindow.Height / 2) - (StandardCanvas.Height / 2));
        }

        private void UpdateWindow(object sender, RoutedEventArgs e)
        {
            StandardCanvas.Height = (mainWindow.Height / 3) * 2 + 100;
            StandardCanvas.Width = (mainWindow.Width / 3) * 2;
            StandardCanvas.Margin = new Thickness((mainWindow.Width / 2) - (StandardCanvas.Width / 2), (mainWindow.Height / 2) - (StandardCanvas.Height / 2), (mainWindow.Width / 2) - (StandardCanvas.Width / 2), (mainWindow.Height / 2) - (StandardCanvas.Height / 2));

        }
    }
}
