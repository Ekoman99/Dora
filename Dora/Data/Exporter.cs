using Microsoft.Win32;
using OxyPlot;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace Dora.Data
{
    static class Exporter
    {
        public static void ExportKmlFile(List<BaseCsvData> dataList, SettingsDefinitions settings)
        {
            string kml = GenerateKml(dataList, settings);

            if (!string.IsNullOrEmpty(kml))
            {
                var saveDialog = new SaveFileDialog
                {
                    Filter = "KML File (*.kml)|*.kml"
                };

                if (saveDialog.ShowDialog() == true)
                {
                    try
                    {
                        File.WriteAllText(saveDialog.FileName, kml);
                        MessageBox.Show("KML file saved successfully.");
                    }
                    catch (IOException ex)
                    {
                        MessageBox.Show($"An error occurred while saving the file: {ex.Message}");
                    }
                }
            }
            else
            {
                MessageBox.Show("Failed to generate KML.");
            }
        }

        private static string GenerateKml(List<BaseCsvData> dataList, SettingsDefinitions settings)
        {
            if (dataList == null || dataList.Count == 0)
            {
                return string.Empty;
            }

            StringBuilder kmlBuilder = new StringBuilder();

            kmlBuilder.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            kmlBuilder.AppendLine("<kml xmlns=\"http://www.opengis.net/kml/2.2\">");
            kmlBuilder.AppendLine("  <Document>");

            // stil za sve
            kmlBuilder.AppendLine("    <Style id=\"polyStyle\">");
            kmlBuilder.AppendLine("      <PolyStyle>");
            kmlBuilder.AppendLine("        <color>7fffffff</color>"); // 7f - 50% white opacity
            kmlBuilder.AppendLine("      </PolyStyle>");
            kmlBuilder.AppendLine("    </Style>");

            // sve što se generira za KML
            var properties = new[] { "PCI", "RSRP", "RSRQ", "SINR", "CQI", "Ping", "Downlink" };

            foreach (var property in properties)
            {
                // min i max
                var propertyValues = dataList.Select(item =>
                {
                    var value = item.GetType().GetProperty(property)?.GetValue(item, null);
                    if (value is int intValue && intValue == int.MaxValue) return 0;
                    return Convert.ToDouble(value);
                }).Where(v => v != null).Cast<double>().ToList();

                if (!propertyValues.Any())
                    continue;

                double minValue = propertyValues.Min();
                double maxValue = propertyValues.Max();
                double maxHeight = settings.MaxRelativeHeight; // relativna visina definirana u postavkama

                kmlBuilder.AppendLine("    <Placemark>");
                kmlBuilder.AppendLine($"      <name>{property}</name>");
                kmlBuilder.AppendLine("      <styleUrl>#polyStyle</styleUrl>");
                kmlBuilder.AppendLine("      <LineString>");
                kmlBuilder.AppendLine("        <altitudeMode>relativeToGround</altitudeMode>");
                kmlBuilder.AppendLine("        <extrude>1</extrude>");
                kmlBuilder.AppendLine("        <coordinates>");

                foreach (var item in dataList)
                {
                    var propertyValue = item.GetType().GetProperty(property)?.GetValue(item, null);
                    if (propertyValue != null)
                    {
                        double value = Convert.ToDouble(propertyValue);
                        if (value == int.MaxValue) value = 0; // da bi se 2,147,483,647 prikazvalo kao 0

                        double relativeValue = NormalizeValue(value, minValue, maxValue, maxHeight);
                        kmlBuilder.AppendLine($"{item.Longitude.ToString(CultureInfo.InvariantCulture)},{item.Latitude.ToString(CultureInfo.InvariantCulture)},{relativeValue.ToString(CultureInfo.InvariantCulture)}");
                    }
                }

                kmlBuilder.AppendLine("        </coordinates>");
                kmlBuilder.AppendLine("      </LineString>");
                kmlBuilder.AppendLine("    </Placemark>");
            }

            kmlBuilder.AppendLine("  </Document>");
            kmlBuilder.AppendLine("</kml>");

            return kmlBuilder.ToString();
        }

        private static double NormalizeValue(double value, double minValue, double maxValue, double maxHeight)
        {
            if (maxValue == minValue)
            {
                return 0; // izbjegavanje dijeljenja s nulom u formuli
            }

            return ((value - minValue) / (maxValue - minValue)) * maxHeight;
        }

        public static void ExportGraph(bool loadComplete, PlotModel exportModel, Dictionary<string, string> lastSavedPaths, SettingsDefinitions settings)
        {
            if (loadComplete == true)
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "PNG Image (*.png)|*.png";
                saveFileDialog.Title = "Export";

                if (lastSavedPaths.ContainsKey(".png"))
                {
                    saveFileDialog.InitialDirectory = lastSavedPaths[".png"];
                }

                saveFileDialog.ShowDialog();

                if (!string.IsNullOrWhiteSpace(saveFileDialog.FileName))
                {
                    using (var stream = File.Create(saveFileDialog.FileName))
                    {
                        exportModel.Background = OxyColor.Parse(settings.GraphBackground); // boja pozadine iz postavki
                        

                        var exporter = new OxyPlot.Wpf.PngExporter { Width = settings.ExportWidth, Height = settings.ExportHeight };
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
