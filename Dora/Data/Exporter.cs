using Microsoft.Win32;
using OxyPlot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Dora.Data
{
    internal class Exporter
    {
        private string KMLBuilder(List<BaseCsvData> list, string dataSelection)
        {
            StringBuilder kmlBuilder = new StringBuilder();

            kmlBuilder.AppendLine(@"<?xml version=""1.0"" encoding=""UTF-8""?>");
            kmlBuilder.AppendLine(@"<kml xmlns=""http://www.opengis.net/kml/2.2"">");
            kmlBuilder.AppendLine(@"  <Document>");

            kmlBuilder.AppendLine(@"    <Style id=""polyStyle"">");
            kmlBuilder.AppendLine(@"      <PolyStyle>");
            kmlBuilder.AppendLine(@"        <color>7fffffff</color>"); // 7f definira 50% opacity
            kmlBuilder.AppendLine(@"      </PolyStyle>");
            kmlBuilder.AppendLine(@"    </Style>");

            kmlBuilder.AppendLine($"    <Placemark>");
            kmlBuilder.AppendLine($"      <name>{"test"}</name>");
            kmlBuilder.AppendLine(@"      <styleUrl>#polyStyle</styleUrl>");
            kmlBuilder.AppendLine(@"      <LineString>");
            kmlBuilder.AppendLine(@"        <altitudeMode>relativeToGround</altitudeMode>");
            kmlBuilder.AppendLine(@"        <extrude>1</extrude>");
            kmlBuilder.AppendLine(@"        <coordinates>");

            for (int i = 0; i < list.Count; i++)
            {
                var item = list[i];
                var propertyInfo = typeof(BaseCsvData).GetProperty(dataSelection);
                object propertyValue = propertyInfo.GetValue(item, null);
                double value = Convert.ToDouble(propertyValue);

                kmlBuilder.AppendLine($"          {list[i].Longitude},{list[i].Latitude},{propertyValue}");
            }

            kmlBuilder.AppendLine(@"        </coordinates>");
            kmlBuilder.AppendLine(@"      </LineString>");
            kmlBuilder.AppendLine(@"    </Placemark>");
            kmlBuilder.AppendLine(@"  </Document>");
            kmlBuilder.AppendLine(@"</kml>");

            return kmlBuilder.ToString();
        }

        public void ExportGraph(bool loadComplete, PlotModel exportModel, Dictionary<string, string> lastSavedPaths)
        {
            if (loadComplete == true)
            {
                // dialog window
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "PNG Image (*.png)|*.png";
                saveFileDialog.Title = "Export Chart as PNG";

                if (lastSavedPaths.ContainsKey(".png"))
                {
                    saveFileDialog.InitialDirectory = lastSavedPaths[".png"];
                }

                saveFileDialog.ShowDialog();

                // kad se zada ime, exportaj
                if (!string.IsNullOrWhiteSpace(saveFileDialog.FileName))
                {
                    using (var stream = File.Create(saveFileDialog.FileName))
                    {
                        /*model.Background = OxyColors.LightGray; // Set background color
                        ((LineSeries)model.Series[0]).Color = OxyColors.Red; // Change line color* --> test za promjenu izgleda grafa */

                        var exporter = new OxyPlot.Wpf.PngExporter { Width = 800, Height = 600 };
                        exporter.Export(exportModel, stream);
                    }
                    lastSavedPaths[".png"] = Path.GetDirectoryName(saveFileDialog.FileName);
                }
            }
            else
            {
                var warningWindow = new UnloadedWarning();
                warningWindow.Show();
            }
        }
    }
}
