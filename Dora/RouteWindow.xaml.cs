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

            // Create and configure the GMapControl
            gmapControl = new GMapControl();

            // Set the dimensions of the GMapControl
            gmapControl.Width = 800; // Adjust as needed
            gmapControl.Height = 460; // Adjust as needed

            // Initialize GMapControl
            gmapControl.MapProvider = GMapProviders.OpenStreetMap;
            GMaps.Instance.Mode = AccessMode.ServerOnly;
            gmapControl.MinZoom = 1;
            gmapControl.MaxZoom = 18;
            gmapControl.Zoom = 12;
            gmapControl.SetPositionByKeywords("Zagreb, Croatia");
            gmapControl.Position = new PointLatLng(coordinates[0].Latitude, coordinates[0].Longitude);

            // Add markers for coordinates
            foreach (var (latitude, longitude) in coordinates)
            {
                GMapMarker marker = new GMapMarker(new PointLatLng(latitude, longitude));
                gmapControl.Markers.Add(marker);
            }

            // Add the GMapControl to the window's grid
            Grid grid = new Grid();
            grid.Children.Add(gmapControl);
            this.Content = grid;

            // Subscribe to the mouse wheel event for zooming
            gmapControl.MouseWheel += GmapControl_MouseWheel;
        }

        private void GmapControl_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            e.Handled = true;

            if (e.Delta > 0)
            {
                // Zoom in
                gmapControl.Zoom += 1;
            }
            else if (e.Delta < 0)
            {
                // Zoom out, with a minimum zoom level
                if (gmapControl.Zoom > 1)
                {
                    gmapControl.Zoom -= 1;
                }
            }
        }
    }
}
