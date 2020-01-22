using Microsoft.Kinect;
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

namespace Ejercicio2
{


    public partial class MainWindow : Window
    {
        private KinectSensor sensor;
        private byte[] colorPixels;
        private WriteableBitmap colorBitmap;

        //Buffer intermedio bytes profundidad
        private DepthImagePixel[] depthPixels;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            foreach (var potentialSensor in KinectSensor.KinectSensors)
            {
                if (potentialSensor.Status == KinectStatus.Connected)
                {
                    sensor = potentialSensor;
                    break;
                }
                else
                {
                    break;
                }
            }

            try
            {
                this.sensor.Start();
            }
            catch (NullReferenceException)
            {
                this.sensor = null;
            }

            if (this.sensor != null)
            {
                sensor.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);
                sensor.DepthFrameReady += SensorDepthFrameReady;
            }
        }

        private void SensorDepthFrameReady(object sender, DepthImageFrameReadyEventArgs e)
        {
            using (DepthImageFrame depthFrame = e.OpenDepthImageFrame())
            {
                this.depthPixels = new DepthImagePixel[this.sensor.DepthStream.FramePixelDataLength];
                this.colorPixels = new byte[this.sensor.DepthStream.FramePixelDataLength * sizeof(int)];
                this.colorBitmap = new WriteableBitmap(this.sensor.DepthStream.FrameWidth, this.sensor.DepthStream.FrameHeight, 96.0, 96.0, PixelFormats.Bgr32, null);

                if (depthFrame != null)
                {
                    depthFrame.CopyDepthImagePixelDataTo(this.depthPixels);

                    //Conversion profunidad RGB
                    int minDepth = depthFrame.MinDepth;
                    int maxDepth = depthFrame.MaxDepth;
                    // Convertir profundidad a RGB
                    int colorPixelIndex = 0;
                    for (int i = 0; i < this.depthPixels.Length; ++i)
                    {
                        short depth = depthPixels[i].Depth;
                        byte intensity = (byte)(depth >= minDepth && depth <= maxDepth ?
                        depth : 0);
                        this.colorPixels[colorPixelIndex++] = intensity;
                        this.colorPixels[colorPixelIndex++] = intensity;
                        this.colorPixels[colorPixelIndex++] = intensity;
                        ++colorPixelIndex; // no alpha channel RGB
                    }

                    //Copiar color pixels a bitmap(visualizacion)
                    // Copiar pixels RGB en el bitmap
                    this.colorBitmap.WritePixels(
                    new Int32Rect(0, 0, this.colorBitmap.PixelWidth,
                    this.colorBitmap.PixelHeight),
                    this.colorPixels,
                    this.colorBitmap.PixelWidth * sizeof(int), 0);

                    image.Source = colorBitmap;
                }
            }
        }

        private void SensorColorFrameReady(object sender, ColorImageFrameReadyEventArgs e)
        {
            using (ColorImageFrame colorFrame = e.OpenColorImageFrame())
            {
                this.colorPixels = new byte[this.sensor.ColorStream.FramePixelDataLength];
                this.colorBitmap = new WriteableBitmap(this.sensor.ColorStream.FrameWidth,this.sensor.ColorStream.FrameHeight, 96.0, 96.0, PixelFormats.Bgr32, null);

                if (colorFrame != null)
                {
                    colorFrame.CopyPixelDataTo(this.colorPixels);
                    this.colorBitmap.WritePixels(new Int32Rect(0, 0, this.colorBitmap.PixelWidth, this.colorBitmap.PixelHeight), this.colorPixels, this.colorBitmap.PixelWidth * sizeof(int), 0);
                    image.Source = colorBitmap;
                }
            }
        }

        private void rbProfundidad_Checked(object sender, RoutedEventArgs e)
        {
            if (this.sensor != null)
            {
                sensor.ColorStream.Disable();

                this.sensor.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);
                this.sensor.DepthFrameReady += this.SensorDepthFrameReady;
            }
        }

        private void rbColor_Checked(object sender, RoutedEventArgs e)
        {
            if (this.sensor != null)
            {
                sensor.DepthStream.Disable();

                sensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);
                sensor.ColorFrameReady += SensorColorFrameReady;
            }
        }
    }
}
