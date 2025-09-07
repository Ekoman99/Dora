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

            gmapControl = new GMapControl
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch
            };

            // default
            gmapControl.MapProvider = GMapProviders.OpenStreetMap;
            GMaps.Instance.Mode = AccessMode.ServerOnly;
            gmapControl.MinZoom = 1;
            gmapControl.MaxZoom = 20;
            gmapControl.Zoom = 14;
            gmapControl.ShowCenter = false;

            // markeri
            foreach (var (latitude, longitude) in coordinates)
            {
                GMapMarker marker = new GMapMarker(new PointLatLng(latitude, longitude))
                {
                    Shape = new Ellipse()
                    {
                        Fill = new SolidColorBrush(Colors.Red),
                        Width = 0, // 0 da se ne vide markeri nego samo linije
                        Height = 4,
                        Margin = new Thickness(-4, -4, 0, 0)
                    }
                };
                gmapControl.Markers.Add(marker);
            }

            List<PointLatLng> routePoints = coordinates.Select(c => new PointLatLng(c.Latitude, c.Longitude)).ToList();

            for (int i = 0; i < routePoints.Count - 1; i++)
            {
                GMapRoute routeSegment = new GMapRoute(new List<PointLatLng> { routePoints[i], routePoints[i + 1] });

                int colorIndex = i % boje.Count;
                string colorName = !string.IsNullOrEmpty(boje[colorIndex].Color) ? boje[colorIndex].Color : "Gray";

                Color selectedColor = (Color)ColorConverter.ConvertFromString(colorName);

                routeSegment.Shape = new Path()
                {
                    Stroke = new SolidColorBrush(selectedColor),
                    StrokeThickness = 3,
                    SnapsToDevicePixels = true,
                    UseLayoutRounding = true
                };

                gmapControl.Markers.Add(routeSegment);
            }

            gmapControl.Position = new PointLatLng(coordinates[0].Latitude, coordinates[0].Longitude);

            MainGrid.Children.Add(gmapControl);

            this.SizeChanged += RouteWindow_SizeChanged; // resizing dynamically

            gmapControl.MouseWheel += GmapControlMouseWheel;

            this.Closed += (sender, e) =>
            {
                gmapControl.Markers.Clear();
                gmapControl.Dispose();
                DisposeMapControl();
            };
        }

        private void RouteWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            gmapControl.Width = e.NewSize.Width;
            gmapControl.Height = e.NewSize.Height;
        }

        private void GmapControlMouseWheel(object sender, MouseWheelEventArgs e)
        {
            e.Handled = true;

            double zoomStep = 0.2;

            if (e.Delta > 0)
            {
                gmapControl.Zoom = Math.Min(gmapControl.MaxZoom, gmapControl.Zoom + zoomStep);
            }
            else if (e.Delta < 0)
            {
                gmapControl.Zoom = Math.Max(gmapControl.MinZoom, gmapControl.Zoom - zoomStep);
            }
        }

        public void DisposeMapControl()
        {
            gmapControl.Dispose();
        }
    }
}
