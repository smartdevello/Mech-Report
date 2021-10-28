using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
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

namespace Mech_Report
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MechRenderer mechRenderer = null;
        private MechManager mechManager = null;
        private int currentChartIndex = 0;
        public MainWindow()
        {
            mechManager = null;
            mechRenderer = null;            
            InitializeComponent();
            seeNext.Visibility = Visibility.Hidden;
            seePrevious.Visibility = Visibility.Hidden;
        }
        private void myCanvas_Loaded(object sender, RoutedEventArgs e)
        {
            Render();
        }
        private void myCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //Render();
        }
        private void btnImportCSV_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "csv files (*.csv)|*.csv|All files (*.*)|*.*";


            if (openFileDialog.ShowDialog() == true)
            {
                if (mechManager == null) mechManager = new MechManager(openFileDialog.FileName);
                else mechManager.setInputfile(openFileDialog.FileName);

                List<MechData> data = mechManager.readData();
                csvFilepath.Text = openFileDialog.FileName;

                if (data != null)
                {
                    Dictionary<string, int> printers = new Dictionary<string, int>();
                    foreach (var item in data)
                    {
                        if (string.IsNullOrEmpty(item.image_id)) continue;
                        if (printers.ContainsKey(item.image_id))
                        {
                            printers[item.image_id]++;
                        }
                        else
                        {
                            printers[item.image_id] = 1;
                        }
                    }

                    mechRenderer.setChatData(data, printers);
                    currentChartIndex = 0;
                    seeNext.Visibility = Visibility.Hidden;
                    seePrevious.Visibility = Visibility.Hidden;
                    Render();

                }
                else
                {
                    string msg = mechManager.getLastException();

                    if (msg == "System.IO.IOException")
                        MessageBox.Show("The file is open by another process", "Error");
                    else if (msg == "CsvHelper.TypeConversion.TypeConverterException")
                    {
                        MessageBox.Show("The file format is invalid, Please check your csv file again.", "Error");
                    } else if (msg == "CsvHelper.HeaderValidationException")
                    {
                        MessageBox.Show("CSV Header is not correct, please make sure you are using the correct CSV file", "Error");
                    }
                }


            }
        }
        private void btnExportChart_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Image file (*.png)|*.png";
            //saveFileDialog.Filter = "Image file (*.png)|*.png|PDF file (*.pdf)|*.pdf";
            if (saveFileDialog.ShowDialog() == true)
            {
                SaveControlImage(MechChart, saveFileDialog.FileName);
            }
        }
        private void drawNextChart(object sender, RoutedEventArgs e)
        {
            int printerCount = mechRenderer.getPrintersCount();
            int chartCount = printerCount / 100;
            if (printerCount % 100 != 0) chartCount++;

            currentChartIndex++;

            if (currentChartIndex == 0)
            {
                seePrevious.IsEnabled = false;
                seeNext.IsEnabled = true;
            } else if (currentChartIndex == chartCount - 1)
            {
                seePrevious.IsEnabled = true;
                seeNext.IsEnabled = false;
            } else
            {
                seePrevious.IsEnabled = true;
                seeNext.IsEnabled = true;
            }
            mechRenderer.draw(currentChartIndex);
            myImage.Source = BmpImageFromBmp(mechRenderer.getBmp());
        }
        private void drawPreviousChart(object sender, RoutedEventArgs e)
        {
            int printerCount = mechRenderer.getPrintersCount();
            int chartCount = printerCount / 100;
            if (printerCount % 100 != 0) chartCount++;

            currentChartIndex--;

            if (currentChartIndex == 0)
            {
                seePrevious.IsEnabled = false;
                seeNext.IsEnabled = true;
            }
            else if (currentChartIndex == chartCount - 1)
            {
                seePrevious.IsEnabled = true;
                seeNext.IsEnabled = false;
            }
            else
            {
                seePrevious.IsEnabled = true;
                seeNext.IsEnabled = true;
            }
            mechRenderer.draw(currentChartIndex);
            myImage.Source = BmpImageFromBmp(mechRenderer.getBmp());
        }

        void Render()
        {
            
            //MechChart.Width = 1500;
            if ( mechRenderer == null)
            {
                mechRenderer = new MechRenderer((int)myCanvas.ActualWidth, (int)myCanvas.ActualHeight);
            }            

            int printerCount = mechRenderer.getPrintersCount();
            if (printerCount != 0 && printerCount > 20)
            {
                int gapCount = printerCount / 20;
                if (printerCount%20 != 0) gapCount++;

                if (gapCount > 5)
                {
                    seeNext.Visibility = Visibility.Visible;
                    seePrevious.Visibility = Visibility.Visible;
                    seePrevious.IsEnabled = false;
                    gapCount = 5;
                }
                Application.Current.MainWindow.Width = 600 + 270 * gapCount;
            }

            mechRenderer.setRenderSize((int)myCanvas.ActualWidth, (int)myCanvas.ActualHeight);

            mechRenderer.draw(currentChartIndex);
            myImage.Source = BmpImageFromBmp(mechRenderer.getBmp());
        }
        private BitmapImage BmpImageFromBmp(Bitmap bmp)
        {
            using (var memory = new System.IO.MemoryStream())
            {
                bmp.Save(memory, System.Drawing.Imaging.ImageFormat.Png);
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
        public BitmapSource CreateBitmapFromControl(FrameworkElement element)
        {
            // Get the size of the Visual and its descendants.
            Rect rect = VisualTreeHelper.GetDescendantBounds(element);

            // Make a DrawingVisual to make a screen
            // representation of the control.
            DrawingVisual dv = new DrawingVisual();

            // Fill a rectangle the same size as the control
            // with a brush containing images of the control.
            using (DrawingContext ctx = dv.RenderOpen())
            {
                VisualBrush brush = new VisualBrush(element);
                ctx.DrawRectangle(brush, null, new Rect(rect.Size));
            }

            // Make a bitmap and draw on it.
            int width = (int)(element.ActualWidth);
            int height = (int)(element.ActualHeight);


            //RenderTargetBitmap rtb = new RenderTargetBitmap(
            //    width, height, 240, 240, PixelFormats.Pbgra32);

            RenderTargetBitmap rtb = new RenderTargetBitmap(
    width, height, 96, 96, PixelFormats.Default);
            rtb.Render(dv);
            return rtb;
        }
        // Save a control's image.
        private void SaveControlImage(FrameworkElement control,
            string filename)
        {
            RenderTargetBitmap rtb = (RenderTargetBitmap)CreateBitmapFromControl(control);
            
            // Make a PNG encoder.
            PngBitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(rtb));

            // Save the file.
            using (FileStream fs = new FileStream(filename,
                FileMode.Create, FileAccess.Write, FileShare.None))
            {
                encoder.Save(fs);
            }
        }

    }
}
