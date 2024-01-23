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
using System.Drawing;
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

            GMapRoute route = new GMapRoute(routePoints);
            route.Shape = new Path() { Stroke = new SolidColorBrush(Colors.Blue), StrokeThickness = 3 };
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

        private void GmapControlMouseWheel(object sender, MouseWheelEventArgs e)
        {
            e.Handled = true;

            if (e.Delta > 0)
            {
                // zoom in
                gmapControl.Zoom += 1;
            }
            else if (e.Delta < 0)
            {
                // zoom out
                if (gmapControl.Zoom > 1)
                {
                    gmapControl.Zoom -= 1;
                }
            }
        }

        public void DisposeMapControl()
        {
            gmapControl.Dispose();
        }
    }
}
