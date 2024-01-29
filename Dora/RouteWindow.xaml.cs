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
using System.Windows.Shapes;
using GMap.NET.MapProviders;
using GMap.NET.WindowsPresentation;
using GMap.NET;
using System.Windows.Media.Effects;

namespace Dora
{
    /// <summary>
    /// Interaction logic for RouteWindow.xaml
    /// </summary>
    public partial class RouteWindow : Window
    {
        private GMapControl gmapControl;

        public RouteWindow(List<(double Latitude, double Longitude)> coordinates)
        {
            InitializeComponent();

            /*List<(double Latitude, double Longitude)> coordinates = 
                list.Select(data => (data.Latitude, data.Longitude)).ToList();*/

            gmapControl = new GMapControl();

            gmapControl.Width = 800;
            gmapControl.Height = 460;

            // init
            gmapControl.MapProvider = GMapProviders.OpenStreetMap;
            GMaps.Instance.Mode = AccessMode.ServerOnly;
            gmapControl.MinZoom = 1;
            gmapControl.MaxZoom = 18;
            gmapControl.Zoom = 14;
            gmapControl.ShowCenter = false;


            // markers
            foreach (var (latitude, longitude) in coordinates)
            {
                GMapMarker marker = new GMapMarker(new PointLatLng(latitude, longitude));
                marker.Shape = new Ellipse()
                {
                    Fill = new SolidColorBrush(Colors.Red),
                    Width = 0, // 0 da se ne vide markeri
                    Height = 4,
                    // centering with margin.
                    Margin = new Thickness(-4, -4, 0, 0)
                };

                gmapControl.Markers.Add(marker);
            }

            // list of points
            List<PointLatLng> routePoints = coordinates.Select(c => new PointLatLng(c.Latitude, c.Longitude)).ToList();

            string colorName = "Blue";
            Color selectedColor = (Color)ColorConverter.ConvertFromString(colorName);

            GMapRoute route = new GMapRoute(routePoints);
            route.Shape = new Path() { Stroke = new SolidColorBrush(selectedColor), StrokeThickness = 3 };
            gmapControl.Markers.Add(route);

            gmapControl.Position = new PointLatLng(coordinates[0].Latitude, coordinates[0].Longitude); //prva pozicija na ruti

            // add GMapControl to grid
            Grid grid = new Grid();
            grid.Children.Add(gmapControl);
            this.Content = grid;

            // zooming
            gmapControl.MouseWheel += GmapControlMouseWheel;

            this.Closed += (sender, e) =>
            {
                DisposeMapControl();
            };
        }

        public RouteWindow(List<(double Latitude, double Longitude)> coordinates, List<(int Id, string Color)> boje)
        {
            InitializeComponent();

            /*List<(double Latitude, double Longitude)> coordinates = 
                list.Select(data => (data.Latitude, data.Longitude)).ToList();*/

            gmapControl = new GMapControl();

            gmapControl.Width = 1280;
            gmapControl.Height = 800;

            // init
            gmapControl.MapProvider = GMapProviders.OpenStreetMap;
            GMaps.Instance.Mode = AccessMode.ServerOnly;
            gmapControl.MinZoom = 1;
            gmapControl.MaxZoom = 20;
            gmapControl.Zoom = 14;
            gmapControl.ShowCenter = false;


            // markers
            foreach (var (latitude, longitude) in coordinates)
            {
                GMapMarker marker = new GMapMarker(new PointLatLng(latitude, longitude));
                marker.Shape = new Ellipse()
                {
                    Fill = new SolidColorBrush(Colors.Red),
                    Width = 0, // 0 da se ne vide markeri
                    Height = 4,
                    // centering with margin.
                    Margin = new Thickness(-4, -4, 0, 0)
                };

                gmapControl.Markers.Add(marker);
            }

            // list of points
            List<PointLatLng> routePoints = coordinates.Select(c => new PointLatLng(c.Latitude, c.Longitude)).ToList();

            for (int i = 0; i < routePoints.Count - 1; i++)
            {
                GMapRoute routeSegment = new GMapRoute(new List<PointLatLng> { routePoints[i], routePoints[i + 1] });

                int colorIndex = i % boje.Count;
                string colorName;
                if (boje[colorIndex].Color != null)
                {
                    colorName = boje[colorIndex].Color;
                }
                else
                {
                    colorName = "Gray";
                }

                Color selectedColor = (Color)ColorConverter.ConvertFromString(colorName);

                // antialiasing za ljepšu crtu
                routeSegment.Shape = new Path()
                {
                    Stroke = new SolidColorBrush(selectedColor),
                    StrokeThickness = 3,
                    SnapsToDevicePixels = true,
                    UseLayoutRounding = true    // improve rendering quality by aligning to layout pixels
                };

                gmapControl.Markers.Add(routeSegment);
            }

            gmapControl.Position = new PointLatLng(coordinates[0].Latitude, coordinates[0].Longitude); //prva pozicija na ruti

            // add GMapControl to grid
            Grid grid = new Grid();
            grid.Children.Add(gmapControl);
            this.Content = grid;

            // zooming
            gmapControl.MouseWheel += GmapControlMouseWheel;

            this.Closed += (sender, e) =>
            {
                DisposeMapControl();
            };
        }

        private void GmapControlMouseWheel(object sender, MouseWheelEventArgs e)
        {
            e.Handled = true;

            double zoomChange = e.Delta > 0 ? 0.5 : -0.5; // Adjusted to 0.5 for finer control

            if (e.Delta > 0)
            {
                // zoom in
                gmapControl.Zoom += zoomChange;
            }
            else if (e.Delta < 0)
            {
                // zoom out
                if (gmapControl.Zoom > 1)
                {
                    gmapControl.Zoom += zoomChange;
                }
            }
        }

        public void DisposeMapControl()
        {
            gmapControl.Dispose();
        }
    }
}
