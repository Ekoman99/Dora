using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using CsvHelper.Expressions;
using CsvHelper.Delegates;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using System.IO;
using System.Globalization;
using CsvHelper.Configuration.Attributes;
using System.Collections;
using Newtonsoft.Json.Linq;
using OxyPlot.Axes;
using OxyPlot;
using OxyPlot.Series;
using System.Linq.Expressions;
using Dora.Data;
using Newtonsoft.Json;
using Dora.UI;
using System.Runtime.InteropServices.ComTypes;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Dora
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;

            DataIntervals = InitializeMapIntervals();
            PlaceInfoCards();
        }

        public string FilePath
        {
            get { return (string)GetValue(filePathCSVProperty); }
            set { SetValue(filePathCSVProperty, value); }
        }

        public static readonly DependencyProperty filePathCSVProperty =
            DependencyProperty.Register("FilePath", typeof(string), typeof(MainWindow), new PropertyMetadata(string.Empty));

        List<BaseCsvData> inputDataList;
        List<(double Latitude, double Longitude)> MainGeoList;

        Dictionary<string, List<MapColorIntervals>> DataIntervals;
        Dictionary<string, string> lastSavedPaths = new Dictionary<string, string>(); //pohrana zadnjeg patha za različite tipove datoteka, implementirano za .csv i .png

        InfoCard greenCard = InfoCard.GreenCardDefault;
        InfoCard blueCard = InfoCard.BlueCardDefault;
        InfoCard redCard = InfoCard.RedCardDefault;

        private bool status4G;
        private bool status5G;
        private bool loadComplete = false;
        bool peakSmooth = true;
        int peakUpperLimit = 50000;
        string tabSelector = "RSRP"; //program prvo učita RSRP

        private bool isOption1Selected;

        public bool IsOption1Selected
        {
            get { return isOption1Selected; }
            set
            {
                if (isOption1Selected != value)
                {
                    isOption1Selected = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void CSVFileSelect(object sender, RoutedEventArgs e)
        {
            bool dataLoadedCSV = false;

            try
            {
                OpenFileDialog openCSVDialog = new OpenFileDialog();
                openCSVDialog.Filter = "CSV Files (*.csv)|*.csv"; // filter za prikaz samo.csv

                if (lastSavedPaths.ContainsKey(".csv"))
                {
                    openCSVDialog.InitialDirectory = lastSavedPaths[".csv"];
                }

                Nullable<bool> result = openCSVDialog.ShowDialog();

                if (result == true)
                {
                    FilePath = openCSVDialog.FileName;
                    dataLoadedCSV = true;
                    loadComplete = true;
                    lastSavedPaths[".csv"] = System.IO.Path.GetDirectoryName(openCSVDialog.FileName);
                }

                inputDataList = LoadCSV(FilePath);

                status4G = Check4G(inputDataList);
                status5G = Check5G(inputDataList);

                MainGeoList = GetCoordinates(inputDataList);

                if (dataLoadedCSV == true)
                {
                    // incijalno pokazivanje RSRP
                    ShowScreen(inputDataList, "RSRP");
                    LineGraph(inputDataList, "RSRP");

                    tabSelector = "RSRP";

                    ChangeButtonStyle(tabSelector);
                    chartTitle.Text = tabSelector;
                }

                Console.WriteLine(inputDataList.Count);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
                FilePath = string.Empty; // filepath ostaje prazan
            }

        }

        public List<BaseCsvData> LoadCSV(string filePath)
        {
            List<BaseCsvData> dataList = new List<BaseCsvData>();

            // definiranje kulture zbog zareza kao separatora
            var commaDecimalCulture = new CultureInfo("hr-HR");

            // pohrana .csv u listu objekata
            var csvConfig = new CsvConfiguration(commaDecimalCulture);
            csvConfig.Delimiter = ";";
            csvConfig.HasHeaderRecord = true; // csv header

            using (var reader = new StreamReader(filePath))
            using (var csv = new CsvReader(reader, csvConfig))
            {
                csv.Context.TypeConverterCache.AddConverter<int?>(new NullableIntTypeConverter());
                dataList = csv.GetRecords<BaseCsvData>().ToList();
            }

            return dataList;
        }

        public class NullableIntTypeConverter : DefaultTypeConverter
        {
            public override object ConvertFromString(string text, IReaderRow row, MemberMapData memberMapData)
            {
                if (string.IsNullOrWhiteSpace(text) || text.Equals("N/A", StringComparison.OrdinalIgnoreCase))
                {
                    return null; // Return null for "N/A" or empty values
                }

                if (int.TryParse(text, out int result))
                {
                    return result; // Return the parsed integer value
                }
                else
                {
                    throw new Exception("errror");
                }
            }
        }

        private Dictionary<string, List<MapColorIntervals>> InitializeMapIntervals()
        {
            string filePath = @"C:\Users\Josip\source\repos\Dora\Dora\Data\MapIntervals.json";
            string json = File.ReadAllText(filePath);

            Dictionary<string, List<MapColorIntervals>> dataIntervals = JsonConvert.DeserializeObject<Dictionary<string, List<MapColorIntervals>>>(json);

            return dataIntervals;
        }

        private void PlaceInfoCards()
        {
            infoCardGrid.Children.Add(greenCard);
            infoCardGrid.Children.Add(blueCard);
            infoCardGrid.Children.Add(redCard);

            Grid.SetColumn(greenCard, 0);
            Grid.SetColumn(blueCard, 1);
            Grid.SetColumn(redCard, 2);
        }

        private void ShowMap(object sender, RoutedEventArgs e)
        {
            if (loadComplete == false)
            {
                var warningWindow = new UnloadedWarning();
                warningWindow.Show();
            }
            else
            {
                if (tabSelector == "Downlink" || tabSelector == "RSRP" || tabSelector == "SINR" || tabSelector == "RSRQ" || tabSelector == "CQI" || tabSelector == "Ping")
                {
                    List<(int Id, string Color)> boje = AssignColors(inputDataList, tabSelector, DataIntervals);

                    var mapWindow = new RouteWindow(MainGeoList, boje);
                    mapWindow.Show();
                }

                else
                {
                    var mapWindow = new RouteWindow(MainGeoList);
                    mapWindow.Show();
                }
            }

        }

        public void ExportGraph(object sender, RoutedEventArgs e)
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
                        exporter.Export(model, stream);
                    }
                    lastSavedPaths[".png"] = System.IO.Path.GetDirectoryName(saveFileDialog.FileName);
                }
            }
            else
            {
                var warningWindow = new UnloadedWarning();
                warningWindow.Show();
            }
        }

        private void ExportKML(object sender, RoutedEventArgs e)
        {
            if(loadComplete == true)
            {
                string kml = KMLGenerator(inputDataList, tabSelector);

                if (!string.IsNullOrEmpty(kml))
                {
                    SaveFileDialog saveKMLDialog = new SaveFileDialog();
                    saveKMLDialog.Filter = "KML File (*.kml)|*.kml";
                    if (saveKMLDialog.ShowDialog() == true)
                    {
                        string fileName = saveKMLDialog.FileName;

                        try
                        {
                            File.WriteAllText(fileName, kml);
                            MessageBox.Show("KML file saved successfully.");
                        }
                        catch (IOException ex)
                        {
                            MessageBox.Show("An error occurred while saving the file: " + ex.Message);
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Failed to generate KML.");
                }
            }
            else
            {
                var warningWindow = new UnloadedWarning();
                warningWindow.Show();
            }
        }

        private void ClickRSRP(object sender, RoutedEventArgs e)
        {
            if (loadComplete == true)
            {
                string dataSelection = "RSRP";
                string unit = "dBm";
                chartTitle.Text = dataSelection;

                double minimumValue = CalculateMaximum(inputDataList, dataSelection);
                greenCard.Number = minimumValue.ToString() + unit;
                double maximumValue = CalculateMinimum(inputDataList, dataSelection);
                redCard.Number = maximumValue.ToString() + unit;
                double averageValue = CalculateAverage(inputDataList, dataSelection);
                blueCard.Number = averageValue.ToString("n2") + unit;
                /* MessageBox.Show("min:" + minimumValue + "\nmax:" + maximumValue + "\navg:" + averageValue); --> samo za test podataka */

                tabSelector = "RSRP";
                UpdateGraph();
                ChangeButtonStyle(tabSelector);
                InfoCardText();
            }
            else
            {
                var warningWindow = new UnloadedWarning();
                warningWindow.Show();
            }
        }

        private void ClickRSRQ(object sender, RoutedEventArgs e)
        {
            if (loadComplete == true)
            {
                string dataSelection = "RSRQ";
                string unit = "dB";
                chartTitle.Text = dataSelection;

                double minimumValue = CalculateMaximum(inputDataList, dataSelection, peakSmooth, peakUpperLimit);
                greenCard.Number = minimumValue.ToString() + unit;
                double maximumValue = CalculateMinimum(inputDataList, dataSelection);
                redCard.Number = maximumValue.ToString() + unit;
                double averageValue = CalculateAverage(inputDataList, dataSelection, peakSmooth, peakUpperLimit);
                blueCard.Number = averageValue.ToString("n2") + unit;
                /* MessageBox.Show("min:" + minimumValue + "\nmax:" + maximumValue + "\navg:" + averageValue); --> samo za test podataka */

                tabSelector = "RSRQ";
                UpdateGraph();
                ChangeButtonStyle(tabSelector);
                InfoCardText();
            }
            else
            {
                var warningWindow = new UnloadedWarning();
                warningWindow.Show();
            }
        }

        private void ClickSINR(object sender, RoutedEventArgs e)
        {
            if (loadComplete == true)
            {
                string dataSelection = "SINR";
                string unit = "dB";
                chartTitle.Text = dataSelection;

                double minimumValue = CalculateMaximum(inputDataList, dataSelection, peakSmooth, peakUpperLimit);
                greenCard.Number = minimumValue.ToString() + unit;
                double maximumValue = CalculateMinimum(inputDataList, dataSelection);
                redCard.Number = maximumValue.ToString() + unit;
                double averageValue = CalculateAverage(inputDataList, dataSelection, peakSmooth, peakUpperLimit);
                blueCard.Number = averageValue.ToString("n2") + unit;
                /* MessageBox.Show("min:" + minimumValue + "\nmax:" + maximumValue + "\navg:" + averageValue); --> samo za test podataka */

                tabSelector = "SINR";
                UpdateGraph();
                ChangeButtonStyle(tabSelector);
                InfoCardText();
            }
            else
            {
                var warningWindow = new UnloadedWarning();
                warningWindow.Show();
            }
        }

        private void ClickCQI(object sender, RoutedEventArgs e)
        {
            if (loadComplete == true)
            {
                string dataSelection = "CQI";
                chartTitle.Text = dataSelection;

                double minimumValue = CalculateMaximum(inputDataList, dataSelection);
                greenCard.Number = minimumValue.ToString() + "";
                double maximumValue = CalculateMinimum(inputDataList, dataSelection);
                redCard.Number = maximumValue.ToString() + "";
                double averageValue = Math.Floor(CalculateAverage(inputDataList, dataSelection)); // CQI je cjelobrojna vrijednost
                blueCard.Number = averageValue.ToString("n0") + "";
                /* MessageBox.Show("min:" + minimumValue + "\nmax:" + maximumValue + "\navg:" + averageValue); --> samo za test podataka */

                tabSelector = "CQI";
                UpdateGraph();
                ChangeButtonStyle(tabSelector);
                InfoCardText();
            }
            else
            {
                var warningWindow = new UnloadedWarning();
                warningWindow.Show();
            }
        }

        private void ClickPCI(object sender, RoutedEventArgs e)
        {
            if (loadComplete == true)
            {
                string dataSelection = "PCI";
                chartTitle.Text = dataSelection;

                double minimumValue = CalculateMaximum(inputDataList, dataSelection);
                greenCard.Number = minimumValue.ToString() + "";
                double maximumValue = CalculateMinimum(inputDataList, dataSelection);
                redCard.Number = maximumValue.ToString() + "";
                blueCard.Number = "N/A";
                /* MessageBox.Show("min:" + minimumValue + "\nmax:" + maximumValue + "\navg:" + averageValue); --> samo za test podataka */

                tabSelector = "PCI";
                UpdateGraph();
                ChangeButtonStyle(tabSelector);
                InfoCardText();
            }
            else
            {
                var warningWindow = new UnloadedWarning();
                warningWindow.Show();
            }
        }

        private void ClickPing(object sender, RoutedEventArgs e)
        {
            if (loadComplete == true)
            {
                string dataSelection = "Ping";
                string unit = "ms";
                chartTitle.Text = dataSelection;

                double minimumValue = CalculateMinimum(inputDataList, dataSelection);
                greenCard.Number = minimumValue.ToString() + unit;
                double maximumValue = CalculateMaximum(inputDataList, dataSelection);
                redCard.Number = maximumValue.ToString() + unit;
                double averageValue = CalculateAverage(inputDataList, dataSelection);
                blueCard.Number = averageValue.ToString("n2") + unit;
                /* MessageBox.Show("min:" + minimumValue + "\nmax:" + maximumValue + "\navg:" + averageValue); --> samo za test podataka */

                tabSelector = "Ping";
                UpdateGraph();
                ChangeButtonStyle(tabSelector);
                InfoCardText();
            }
            else
            {
                var warningWindow = new UnloadedWarning();
                warningWindow.Show();
            }
        }

        private void ClickThroughput(object sender, RoutedEventArgs e)
        {
            if (loadComplete == true)
            {
                string dataSelection = "Downlink";
                string unit = "Mbps";
                chartTitle.Text = dataSelection;

                double minimumValue = CalculateMaximum(inputDataList, dataSelection);
                greenCard.Number = (minimumValue * 8).ToString("n2") + unit;
                double maximumValue = CalculateMinimum(inputDataList, dataSelection);
                redCard.Number = (maximumValue * 8).ToString("n2") + unit;
                double averageValue = CalculateAverage(inputDataList, dataSelection);
                blueCard.Number = (averageValue * 8).ToString("n2") + unit;
                /* MessageBox.Show("min:" + minimumValue + "\nmax:" + maximumValue + "\navg:" + averageValue); --> samo za test podataka */

                tabSelector = "Downlink";
                UpdateGraph();
                ChangeButtonStyle(tabSelector);
                InfoCardText();
            }
            else
            {
                var warningWindow = new UnloadedWarning();
                warningWindow.Show();
            }
        }

        private void Logoff(object sender, RoutedEventArgs e)
        {
            var login = new Login();
            login.Show();
            this.Close();
        }

        private double CalculateAverage(List<BaseCsvData> list, string dataSelection)
        {
            double sum = 0;
            int count = 0;

            if (list.Count > 0)
            {
                foreach (var item in list)
                {
                    var propertyInfo = typeof(BaseCsvData).GetProperty(dataSelection);
                    if (propertyInfo != null)
                    {
                        object propertyValue = propertyInfo.GetValue(item, null);
                        if (propertyValue != null && (propertyValue is double || propertyValue is int || propertyValue is float))
                        {
                            sum += Convert.ToDouble(propertyValue);
                            count++;
                        }
                    }
                }
            }

            return count > 0 ? sum / count : 0; // div0
        }

        private double CalculateAverage(List<BaseCsvData> list, string dataSelection, bool peakNormalization, int peakLimit)
        {
            double sum = 0;
            int count = 0;

            if (list.Count > 0)
            {
                foreach (var item in list)
                {
                    var propertyInfo = typeof(BaseCsvData).GetProperty(dataSelection);
                    if (propertyInfo != null)
                    {
                        object propertyValue = propertyInfo.GetValue(item, null);
                        if (propertyValue != null && (propertyValue is double || propertyValue is int || propertyValue is float))
                        {
                            if (Convert.ToInt32(propertyValue) < peakLimit && peakNormalization == true)
                            {
                                sum += Convert.ToDouble(propertyValue);
                                count++;
                            }
                        }
                    }
                }
            }

            return count > 0 ? sum / count : 0; // div0
        }

        private double CalculateMinimum(List<BaseCsvData> list, string dataSelection)
        {
            double min = double.MaxValue;

            if (list.Count > 0)
            {
                min = list.Where(item =>
                {
                    var propertyInfo = typeof(BaseCsvData).GetProperty(dataSelection);
                    if (propertyInfo != null)
                    {
                        object propertyValue = propertyInfo.GetValue(item, null);
                        if (propertyValue != null && (propertyValue is double || propertyValue is int || propertyValue is float))
                        {
                            return true; // samo numeričke vrijednosti
                        }
                    }
                    return false;
                })
                .Min(item =>
                {
                    var propertyInfo = typeof(BaseCsvData).GetProperty(dataSelection);
                    object propertyValue = propertyInfo.GetValue(item, null);
                    return Convert.ToDouble(propertyValue);
                });
            }

            return min;
        }

        private double CalculateMaximum(List<BaseCsvData> list, string dataSelection)
        {
            double max = 0;

            if (list.Count > 0)
            {
                max = list.Max(item =>
                {
                    var propertyInfo = typeof(BaseCsvData).GetProperty(dataSelection);
                    if (propertyInfo != null)
                    {
                        object propertyValue = propertyInfo.GetValue(item, null);
                        if (propertyValue is double || propertyValue is int || propertyValue is float)
                        {
                            return Convert.ToDouble(propertyValue);
                        }
                    }
                    return 0;
                });
            }

            return max;
        }

        private double CalculateMaximum(List<BaseCsvData> list, string dataSelection, bool peakNormalization, int peakLimit)
        {
            double max = 0;

            if (list.Count > 0)
            {
                max = list.Max(item =>
                {
                    var propertyInfo = typeof(BaseCsvData).GetProperty(dataSelection);
                    if (propertyInfo != null)
                    {
                        object propertyValue = propertyInfo.GetValue(item, null);
                        if (propertyValue is double || propertyValue is int || propertyValue is float)
                        {
                            if (Convert.ToInt32(propertyValue) < peakLimit && peakNormalization == true)
                            {
                                return Convert.ToDouble(propertyValue);
                            }
                        }
                    }
                    return 0;
                });
            }

            return max;
        }

        private void FillInfoCards(string dataSelection, string unit)
        {
            double minimumValue = CalculateMaximum(inputDataList, dataSelection);
            greenCard.Number = (minimumValue * 8).ToString("n2") + unit;
            double maximumValue = CalculateMinimum(inputDataList, dataSelection);
            redCard.Number = (maximumValue * 8).ToString("n2") + unit;
            double averageValue = CalculateAverage(inputDataList, dataSelection);
            blueCard.Number = (averageValue * 8).ToString("n2") + unit;
        }

        private void ShowScreen(List<BaseCsvData> inputDataList, string dataSelection)
        {
            double minimumValue = CalculateMaximum(inputDataList, dataSelection);
            greenCard.Number = minimumValue.ToString() + "dBm";
            double maximumValue = CalculateMinimum(inputDataList, dataSelection);
            redCard.Number = maximumValue.ToString() + "dBm";
            double averageValue = CalculateAverage(inputDataList, dataSelection);
            blueCard.Number = averageValue.ToString("n2") + "dBm";
            /* MessageBox.Show("min:" + minimumValue + "\nmax:" + maximumValue + "\navg:" + averageValue); --> samo za test podataka */
        }

        private PlotModel model; //model mora biti dostupan klasi zbog interakcije metoda grafa i exportera

        private void UpdateGraph()
        {
            if(loadComplete == true)
            {
                if (IsOption1Selected && (tabSelector == "RSRQ" || tabSelector == "SINR"))
                {
                    LineGraph(inputDataList, tabSelector, peakSmooth, peakUpperLimit); // Execute LineGraph method if toggle button is on
                }
                else if (IsOption1Selected && !(tabSelector == "RSRQ" || tabSelector == "SINR"))
                {
                    LineGraph(inputDataList, tabSelector);
                }
                else if (!IsOption1Selected && (tabSelector == "RSRQ" || tabSelector == "SINR"))
                {
                    StemGraph(inputDataList, tabSelector, peakSmooth, peakUpperLimit); // Execute StemGraph method if toggle button is off
                }
                else
                {
                    StemGraph(inputDataList, tabSelector);
                }
            }
            else
            {
                var warningWindow = new UnloadedWarning();
                warningWindow.Show();
            }
        }

        public void LineGraph(List<BaseCsvData> inputList, string dataSelection)
        {
            // kreiranje modela za plotanje
            model = new PlotModel
            {
                Background = OxyColors.Transparent,
                PlotAreaBorderColor = OxyColors.Transparent,
            };

            // serija točaka
            var seriesBlue = new LineSeries // 4G
            {
                Color = OxyColor.Parse("#349DC8"),
            };

            var seriesRed = new LineSeries // 5G
            {
                Color = OxyColor.Parse("#C41F1F"),
            };

            for (int i = 0; i < inputList.Count; i++) // ---> test za dualno pokazivanje grafa
            {
                if (inputList[i].Tech == "EN-DC")
                {
                    // uzimanje vrijednosti, dataSelection definira koji property 
                    object dataValue = inputList[i].GetType().GetProperty(dataSelection).GetValue(inputList[i]);

                    if (dataValue != null)
                    {
                        seriesRed.Points.Add(new DataPoint(DateTimeAxis.ToDouble(inputList[i].Time), Convert.ToDouble(dataValue))); // vrijednost je 5G, upisujem
                        seriesBlue.Points.Add(new DataPoint(DateTimeAxis.ToDouble(inputList[i].Time), double.NaN)); // u 4G upisujem nullove
                    }
                    else
                    {
                        seriesRed.Points.Add(new DataPoint(DateTimeAxis.ToDouble(inputList[i].Time), double.NaN)); // nema 5G vrijednosti                        
                    }
                }
                else
                {
                    object dataValue = inputList[i].GetType().GetProperty(dataSelection).GetValue(inputList[i]);

                    if (dataValue != null)
                    {
                        seriesBlue.Points.Add(new DataPoint(DateTimeAxis.ToDouble(inputList[i].Time), Convert.ToDouble(dataValue))); // vrijednost je 4G, upisujem
                        seriesRed.Points.Add(new DataPoint(DateTimeAxis.ToDouble(inputList[i].Time), double.NaN)); // u 5G upisujem nullove
                    }
                    else
                    {
                        seriesBlue.Points.Add(new DataPoint(DateTimeAxis.ToDouble(inputList[i].Time), double.NaN)); // nema 4G vrijednosti
                    }
                }

            }

            model.Series.Add(seriesBlue);
            model.Series.Add(seriesRed);

            // definiranje osi
            var xAxis = new DateTimeAxis
            {
                Position = AxisPosition.Bottom,
                Title = "Time", // Optional axis title
                MajorGridlineColor = OxyColor.FromAColor(50, OxyColors.White), // White gridlines
                MajorGridlineStyle = LineStyle.Solid, // Gridline style
                AxislineColor = OxyColor.FromRgb(255, 255, 255), // White axis line
                TitleColor = OxyColor.FromRgb(255, 255, 255), // Axis title color
                TextColor = OxyColor.FromRgb(255, 255, 255), // Axis label color
                MinorTicklineColor = OxyColor.FromRgb(255, 255, 255), // Tick marks color
                TicklineColor = OxyColor.FromRgb(255, 255, 255), // Tick marks color
            };
            var yAxis = new LinearAxis
            {
                Position = AxisPosition.Left,
                Title = dataSelection,
                MajorGridlineColor = OxyColor.FromAColor(50, OxyColors.White),
                MajorGridlineStyle = LineStyle.Solid,
                AxislineColor = OxyColor.FromRgb(255, 255, 255),
                TitleColor = OxyColor.FromRgb(255, 255, 255),
                TextColor = OxyColor.FromRgb(255, 255, 255),
                MinorTicklineColor = OxyColor.FromRgb(255, 255, 255),
                TicklineColor = OxyColor.FromRgb(255, 255, 255),
            };

            model.Axes.Add(xAxis);
            model.Axes.Add(yAxis);

            // zamjena plota, ako postoji
            var oxyplotChart = new OxyPlot.Wpf.PlotView
            {
                Model = model,
                Background = Brushes.Transparent
            };

            oxyplotChartContainer.Children.Clear();
            oxyplotChartContainer.Children.Add(oxyplotChart);
        }

        public void LineGraph(List<BaseCsvData> inputList, string dataSelection, bool peakNormalization, int peakLimit)
        {
            // kreiranje modela za plotanje
            model = new PlotModel
            {
                Background = OxyColors.Transparent,
                PlotAreaBorderColor = OxyColors.Transparent,
            };

            // serija točaka
            var seriesBlue = new LineSeries // 4G
            {
                Color = OxyColor.Parse("#349DC8"),
            };

            var seriesRed = new LineSeries // 5G
            {
                Color = OxyColor.Parse("#C41F1F"),
            };

            for (int i = 0; i < this.inputDataList.Count; i++)
            {
                // uzimanje vrijednosti, dataSelection definira koji property 
                object dataValue = this.inputDataList[i].GetType().GetProperty(dataSelection).GetValue(this.inputDataList[i]);

                if (this.inputDataList[i].Tech == "EN-DC")
                {
                    if (dataValue != null)
                    {
                        if (peakNormalization == true && (int)dataValue < peakLimit)
                        {
                            seriesRed.Points.Add(new DataPoint(DateTimeAxis.ToDouble(this.inputDataList[i].Time), Convert.ToDouble(dataValue))); // vrijednost je 5G i zadovoljava uvjete
                            seriesBlue.Points.Add(new DataPoint(DateTimeAxis.ToDouble(this.inputDataList[i].Time), double.NaN)); // u 4G upisujem nullove
                        }
                        else
                        {
                            seriesRed.Points.Add(new DataPoint(DateTimeAxis.ToDouble(this.inputDataList[i].Time), double.NaN)); // 5G vrijednost ne zadovoljava uvjete
                        }
                    }
                    else
                    {
                        seriesRed.Points.Add(new DataPoint(DateTimeAxis.ToDouble(this.inputDataList[i].Time), double.NaN)); // nema 5G vrijednosti
                    }
                }
                else
                {
                    if (dataValue != null)
                    {
                        if (peakNormalization == true && (int)dataValue < peakLimit)
                        {
                            seriesBlue.Points.Add(new DataPoint(DateTimeAxis.ToDouble(this.inputDataList[i].Time), Convert.ToDouble(dataValue))); // vrijednost je 4G i zadovoljava uvjete
                            seriesRed.Points.Add(new DataPoint(DateTimeAxis.ToDouble(this.inputDataList[i].Time), double.NaN)); // u 5G upisujem nullove
                        }
                        else
                        {
                            seriesBlue.Points.Add(new DataPoint(DateTimeAxis.ToDouble(this.inputDataList[i].Time), double.NaN)); // 4G vrijednost ne zadovoljava uvjete
                        }
                    }
                    else
                    {
                        seriesBlue.Points.Add(new DataPoint(DateTimeAxis.ToDouble(this.inputDataList[i].Time), double.NaN)); // nema 4G vrijednosti
                    }
                }

            }

            model.Series.Add(seriesBlue);
            model.Series.Add(seriesRed);

            // definiranje osi
            var xAxis = new DateTimeAxis
            {
                Position = AxisPosition.Bottom,
                Title = "Time", // Optional axis title
                MajorGridlineColor = OxyColor.FromAColor(50, OxyColors.White), // White gridlines
                MajorGridlineStyle = LineStyle.Solid, // Gridline style
                AxislineColor = OxyColor.FromRgb(255, 255, 255), // White axis line
                TitleColor = OxyColor.FromRgb(255, 255, 255), // Axis title color
                TextColor = OxyColor.FromRgb(255, 255, 255), // Axis label color
                MinorTicklineColor = OxyColor.FromRgb(255, 255, 255), // Tick marks color
                TicklineColor = OxyColor.FromRgb(255, 255, 255), // Tick marks color
            };
            var yAxis = new LinearAxis
            {
                Position = AxisPosition.Left,
                Title = dataSelection,
                MajorGridlineColor = OxyColor.FromAColor(50, OxyColors.White),
                MajorGridlineStyle = LineStyle.Solid,
                AxislineColor = OxyColor.FromRgb(255, 255, 255),
                TitleColor = OxyColor.FromRgb(255, 255, 255),
                TextColor = OxyColor.FromRgb(255, 255, 255),
                MinorTicklineColor = OxyColor.FromRgb(255, 255, 255),
                TicklineColor = OxyColor.FromRgb(255, 255, 255),
            };

            model.Axes.Add(xAxis);
            model.Axes.Add(yAxis);

            // zamjena plota, ako postoji
            var oxyplotChart = new OxyPlot.Wpf.PlotView
            {
                Model = model,
                Background = Brushes.Transparent
            };

            oxyplotChartContainer.Children.Clear();
            oxyplotChartContainer.Children.Add(oxyplotChart);
        }

        public void StemGraph(List<BaseCsvData> inputList, string dataSelection) // izgleda kao clustered column, minimalna prilagodba potrebna
        {
            // kreiranje modela za plotanje
            model = new PlotModel
            {
                Background = OxyColors.Transparent,
                PlotAreaBorderColor = OxyColors.Transparent,
            };

            // serija točaka
            var seriesBlue = new StemSeries // 4G
            {
                Color = OxyColor.Parse("#349DC8"),
            };

            var seriesRed = new StemSeries // 5G
            {
                Color = OxyColor.Parse("#C41F1F"),
            };

            for (int i = 0; i < this.inputDataList.Count; i++) // ---> test za dualno pokazivanje grafa
            {
                if (this.inputDataList[i].Tech == "EN-DC")
                {
                    // uzimanje vrijednosti, dataSelection definira koji property 
                    object dataValue = this.inputDataList[i].GetType().GetProperty(dataSelection).GetValue(this.inputDataList[i]);

                    if (dataValue != null)
                    {
                        seriesRed.Points.Add(new DataPoint(DateTimeAxis.ToDouble(this.inputDataList[i].Time), Convert.ToDouble(dataValue))); // vrijednost je 5G, upisujem
                        seriesBlue.Points.Add(new DataPoint(DateTimeAxis.ToDouble(this.inputDataList[i].Time), double.NaN)); // u 4G upisujem nullove
                    }
                    else
                    {
                        seriesRed.Points.Add(new DataPoint(DateTimeAxis.ToDouble(this.inputDataList[i].Time), double.NaN)); // nema 5G vrijednosti                        
                    }
                }
                else
                {
                    object dataValue = this.inputDataList[i].GetType().GetProperty(dataSelection).GetValue(this.inputDataList[i]);

                    if (dataValue != null)
                    {
                        seriesBlue.Points.Add(new DataPoint(DateTimeAxis.ToDouble(this.inputDataList[i].Time), Convert.ToDouble(dataValue))); // vrijednost je 4G, upisujem
                        seriesRed.Points.Add(new DataPoint(DateTimeAxis.ToDouble(this.inputDataList[i].Time), double.NaN)); // u 5G upisujem nullove
                    }
                    else
                    {
                        seriesBlue.Points.Add(new DataPoint(DateTimeAxis.ToDouble(this.inputDataList[i].Time), double.NaN)); // nema 4G vrijednosti
                    }
                }

            }

            model.Series.Add(seriesBlue);
            model.Series.Add(seriesRed);

            // definiranje osi
            var xAxis = new DateTimeAxis
            {
                Position = AxisPosition.Bottom,
                Title = "Time", // Optional axis title
                MajorGridlineColor = OxyColor.FromAColor(50, OxyColors.White), // White gridlines
                MajorGridlineStyle = LineStyle.Solid, // Gridline style
                AxislineColor = OxyColor.FromRgb(255, 255, 255), // White axis line
                TitleColor = OxyColor.FromRgb(255, 255, 255), // Axis title color
                TextColor = OxyColor.FromRgb(255, 255, 255), // Axis label color
                MinorTicklineColor = OxyColor.FromRgb(255, 255, 255), // Tick marks color
                TicklineColor = OxyColor.FromRgb(255, 255, 255), // Tick marks color
            };
            var yAxis = new LinearAxis
            {
                Position = AxisPosition.Left,
                Title = dataSelection,
                MajorGridlineColor = OxyColor.FromAColor(50, OxyColors.White),
                MajorGridlineStyle = LineStyle.Solid,
                AxislineColor = OxyColor.FromRgb(255, 255, 255),
                TitleColor = OxyColor.FromRgb(255, 255, 255),
                TextColor = OxyColor.FromRgb(255, 255, 255),
                MinorTicklineColor = OxyColor.FromRgb(255, 255, 255),
                TicklineColor = OxyColor.FromRgb(255, 255, 255),
            };

            model.Axes.Add(xAxis);
            model.Axes.Add(yAxis);

            // zamjena plota, ako postoji
            var oxyplotChart = new OxyPlot.Wpf.PlotView
            {
                Model = model,
                Background = Brushes.Transparent
            };

            oxyplotChartContainer.Children.Clear();
            oxyplotChartContainer.Children.Add(oxyplotChart);
        }

        public void StemGraph(List<BaseCsvData> inputList, string dataSelection, bool peakNormalization, int peakLimit)
        {
            // kreiranje modela za plotanje
            model = new PlotModel
            {
                Background = OxyColors.Transparent,
                PlotAreaBorderColor = OxyColors.Transparent,
            };

            // serija točaka
            var seriesBlue = new StemSeries // 4G
            {
                Color = OxyColor.Parse("#349DC8"),
            };

            var seriesRed = new StemSeries // 5G
            {
                Color = OxyColor.Parse("#C41F1F"),
            };

            for (int i = 0; i < this.inputDataList.Count; i++)
            {
                // uzimanje vrijednosti, dataSelection definira koji property 
                object dataValue = this.inputDataList[i].GetType().GetProperty(dataSelection).GetValue(this.inputDataList[i]);

                if (this.inputDataList[i].Tech == "EN-DC")
                {
                    if (dataValue != null)
                    {
                        if (peakNormalization == true && (int)dataValue < peakLimit)
                        {
                            seriesRed.Points.Add(new DataPoint(DateTimeAxis.ToDouble(this.inputDataList[i].Time), Convert.ToDouble(dataValue))); // vrijednost je 5G i zadovoljava uvjete
                            seriesBlue.Points.Add(new DataPoint(DateTimeAxis.ToDouble(this.inputDataList[i].Time), double.NaN)); // u 4G upisujem nullove
                        }
                        else
                        {
                            seriesRed.Points.Add(new DataPoint(DateTimeAxis.ToDouble(this.inputDataList[i].Time), double.NaN)); // 5G vrijednost ne zadovoljava uvjete
                        }
                    }
                    else
                    {
                        seriesRed.Points.Add(new DataPoint(DateTimeAxis.ToDouble(this.inputDataList[i].Time), double.NaN)); // nema 5G vrijednosti
                    }
                }
                else
                {
                    if (dataValue != null)
                    {
                        if (peakNormalization == true && (int)dataValue < peakLimit)
                        {
                            seriesBlue.Points.Add(new DataPoint(DateTimeAxis.ToDouble(this.inputDataList[i].Time), Convert.ToDouble(dataValue))); // vrijednost je 4G i zadovoljava uvjete
                            seriesRed.Points.Add(new DataPoint(DateTimeAxis.ToDouble(this.inputDataList[i].Time), double.NaN)); // u 5G upisujem nullove
                        }
                        else
                        {
                            seriesBlue.Points.Add(new DataPoint(DateTimeAxis.ToDouble(this.inputDataList[i].Time), double.NaN)); // 4G vrijednost ne zadovoljava uvjete
                        }
                    }
                    else
                    {
                        seriesBlue.Points.Add(new DataPoint(DateTimeAxis.ToDouble(this.inputDataList[i].Time), double.NaN)); // nema 4G vrijednosti
                    }
                }

            }

            model.Series.Add(seriesBlue);
            model.Series.Add(seriesRed);

            // definiranje osi
            var xAxis = new DateTimeAxis
            {
                Position = AxisPosition.Bottom,
                Title = "Time", // Optional axis title
                MajorGridlineColor = OxyColor.FromAColor(50, OxyColors.White), // White gridlines
                MajorGridlineStyle = LineStyle.Solid, // Gridline style
                AxislineColor = OxyColor.FromRgb(255, 255, 255), // White axis line
                TitleColor = OxyColor.FromRgb(255, 255, 255), // Axis title color
                TextColor = OxyColor.FromRgb(255, 255, 255), // Axis label color
                MinorTicklineColor = OxyColor.FromRgb(255, 255, 255), // Tick marks color
                TicklineColor = OxyColor.FromRgb(255, 255, 255), // Tick marks color
            };
            var yAxis = new LinearAxis
            {
                Position = AxisPosition.Left,
                Title = dataSelection,
                MajorGridlineColor = OxyColor.FromAColor(50, OxyColors.White),
                MajorGridlineStyle = LineStyle.Solid,
                AxislineColor = OxyColor.FromRgb(255, 255, 255),
                TitleColor = OxyColor.FromRgb(255, 255, 255),
                TextColor = OxyColor.FromRgb(255, 255, 255),
                MinorTicklineColor = OxyColor.FromRgb(255, 255, 255),
                TicklineColor = OxyColor.FromRgb(255, 255, 255),
            };

            model.Axes.Add(xAxis);
            model.Axes.Add(yAxis);

            // zamjena plota, ako postoji
            var oxyplotChart = new OxyPlot.Wpf.PlotView
            {
                Model = model,
                Background = Brushes.Transparent
            };

            oxyplotChartContainer.Children.Clear();
            oxyplotChartContainer.Children.Add(oxyplotChart);
        }

        private void AreaGraph(List<BaseCsvData> inputList, string dataSelection)
        {
            // kreiranje modela za plotanje
            model = new PlotModel
            {
                Background = OxyColors.Transparent,
                PlotAreaBorderColor = OxyColors.Transparent,
            };

            // serija točaka
            var seriesBlue = new AreaSeries // 4G network
            {
                Color = OxyColor.FromArgb(255, 52, 157, 200), // #349DC8
                StrokeThickness = 0,
                Fill = OxyColor.FromArgb(127, 52, 157, 200), // #349DC880
            };

            var seriesRed = new AreaSeries // 5G network
            {
                Color = OxyColor.FromArgb(255, 196, 31, 31), // #C41F1F
                StrokeThickness = 0,
                Fill = OxyColor.FromArgb(127, 196, 31, 31), // #C41F1F80
            };

            for (int i = 0; i < this.inputDataList.Count - 1; i++)
            {
                // Calculate time difference between consecutive points
                double timeDiff = (this.inputDataList[i + 1].Time - this.inputDataList[i].Time).TotalSeconds;

                if (timeDiff > 1.5)
                {
                    // Calculate the number of steps to add based on the time difference
                    int steps = (int)Math.Floor(timeDiff / 1.5);

                    // Add zeros and intermediate timestamps to both series
                    for (int j = 0; j < steps; j++)
                    {
                        DateTime intermediateTime = this.inputDataList[i].Time.AddSeconds((j + 1) * 1.5);

                        seriesBlue.Points.Add(new DataPoint(DateTimeAxis.ToDouble(intermediateTime), 0));
                        seriesRed.Points.Add(new DataPoint(DateTimeAxis.ToDouble(intermediateTime), 0));
                    }
                }

                // Add the current data point
                if (this.inputDataList[i].Tech == "EN-DC")
                {
                    object dataValue = this.inputDataList[i].GetType().GetProperty(dataSelection).GetValue(this.inputDataList[i]);

                    if (dataValue != null)
                    {
                        seriesRed.Points.Add(new DataPoint(DateTimeAxis.ToDouble(this.inputDataList[i].Time), Convert.ToDouble(dataValue)));
                        seriesBlue.Points.Add(new DataPoint(DateTimeAxis.ToDouble(this.inputDataList[i].Time), 0));
                    }
                    else
                    {
                        seriesRed.Points.Add(new DataPoint(DateTimeAxis.ToDouble(this.inputDataList[i].Time), 0));
                        seriesBlue.Points.Add(new DataPoint(DateTimeAxis.ToDouble(this.inputDataList[i].Time), 0));
                    }
                }
                else
                {
                    object dataValue = this.inputDataList[i].GetType().GetProperty(dataSelection).GetValue(this.inputDataList[i]);

                    if (dataValue != null)
                    {
                        seriesBlue.Points.Add(new DataPoint(DateTimeAxis.ToDouble(this.inputDataList[i].Time), Convert.ToDouble(dataValue)));
                        seriesRed.Points.Add(new DataPoint(DateTimeAxis.ToDouble(this.inputDataList[i].Time), 0));
                    }
                    else
                    {
                        seriesBlue.Points.Add(new DataPoint(DateTimeAxis.ToDouble(this.inputDataList[i].Time), 0));
                        seriesRed.Points.Add(new DataPoint(DateTimeAxis.ToDouble(this.inputDataList[i].Time), 0));
                    }
                }
            }

            // Add the last data point
            int lastIndex = this.inputDataList.Count - 1;
            if (this.inputDataList[lastIndex].Tech == "EN-DC")
            {
                object dataValue = this.inputDataList[lastIndex].GetType().GetProperty(dataSelection).GetValue(this.inputDataList[lastIndex]);

                if (dataValue != null)
                {
                    seriesRed.Points.Add(new DataPoint(DateTimeAxis.ToDouble(this.inputDataList[lastIndex].Time), Convert.ToDouble(dataValue)));
                    seriesBlue.Points.Add(new DataPoint(DateTimeAxis.ToDouble(this.inputDataList[lastIndex].Time), 0));
                }
                else
                {
                    seriesRed.Points.Add(new DataPoint(DateTimeAxis.ToDouble(this.inputDataList[lastIndex].Time), 0));
                    seriesBlue.Points.Add(new DataPoint(DateTimeAxis.ToDouble(this.inputDataList[lastIndex].Time), 0));
                }
            }
            else
            {
                object dataValue = this.inputDataList[lastIndex].GetType().GetProperty(dataSelection).GetValue(this.inputDataList[lastIndex]);

                if (dataValue != null)
                {
                    seriesBlue.Points.Add(new DataPoint(DateTimeAxis.ToDouble(this.inputDataList[lastIndex].Time), Convert.ToDouble(dataValue)));
                    seriesRed.Points.Add(new DataPoint(DateTimeAxis.ToDouble(this.inputDataList[lastIndex].Time), 0));
                }
                else
                {
                    seriesBlue.Points.Add(new DataPoint(DateTimeAxis.ToDouble(this.inputDataList[lastIndex].Time), 0));
                    seriesRed.Points.Add(new DataPoint(DateTimeAxis.ToDouble(this.inputDataList[lastIndex].Time), 0));
                }
            }

            model.Series.Add(seriesBlue);
            model.Series.Add(seriesRed);

            // definiranje osi
            var xAxis = new DateTimeAxis
            {
                Position = AxisPosition.Bottom,
                Title = "Time", // Optional axis title
                MajorGridlineColor = OxyColor.FromAColor(50, OxyColors.White), // White gridlines
                MajorGridlineStyle = LineStyle.Solid, // Gridline style
                AxislineColor = OxyColor.FromRgb(255, 255, 255), // White axis line
                TitleColor = OxyColor.FromRgb(255, 255, 255), // Axis title color
                TextColor = OxyColor.FromRgb(255, 255, 255), // Axis label color
                MinorTicklineColor = OxyColor.FromRgb(255, 255, 255), // Tick marks color
                TicklineColor = OxyColor.FromRgb(255, 255, 255), // Tick marks color
            };
            var yAxis = new LinearAxis
            {
                Position = AxisPosition.Left,
                Title = dataSelection,
                MajorGridlineColor = OxyColor.FromAColor(50, OxyColors.White),
                MajorGridlineStyle = LineStyle.Solid,
                AxislineColor = OxyColor.FromRgb(255, 255, 255),
                TitleColor = OxyColor.FromRgb(255, 255, 255),
                TextColor = OxyColor.FromRgb(255, 255, 255),
                MinorTicklineColor = OxyColor.FromRgb(255, 255, 255),
                TicklineColor = OxyColor.FromRgb(255, 255, 255),
            };

            model.Axes.Add(xAxis);
            model.Axes.Add(yAxis);

            // zamjena plota, ako postoji
            var oxyplotChart = new OxyPlot.Wpf.PlotView
            {
                Model = model,
                Background = Brushes.Transparent
            };

            oxyplotChartContainer.Children.Clear();
            oxyplotChartContainer.Children.Add(oxyplotChart);
        } // potrebna prilagodba, area tip nije najbolji zbog tipa podataka        

        private bool Check5G(List<BaseCsvData> list)
        {
            if(list.Any(var => var.Tech == "EN-DC"))
            {
                return true;
            }
            else return false;
        }

        private bool Check4G(List<BaseCsvData> list)
        {
            if (list.Any(var => var.Tech == "LTE CA" || var.Tech == "LTE FDD"))
            {
                return true;
            }
            else return false;
        }

        private List<(double Latitude, double Longitude)> GetCoordinates(List<BaseCsvData> list)
        {
            List<(double Latitude, double Longitude)> coordinatesList = new List<(double, double)>();

            foreach (var item in list)
            {
                double latitude = item.Latitude;
                double longitude = item.Longitude;
                coordinatesList.Add((latitude, longitude));
            }

            return coordinatesList;
        }

        private List<(int Id, string Color)> AssignColors(List<BaseCsvData> list, string dataSelection)
        {
            //trenutno za throughtput

            List<(int Id, string Color)> colorAssignments = new List<(int Id, string Color)>();

            if (list.Count > 0)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    var item = list[i];
                    var propertyInfo = typeof(BaseCsvData).GetProperty(dataSelection);
                    if (propertyInfo != null)
                    {
                        int id = i;
                        string color = "Red";

                        object propertyValue = propertyInfo.GetValue(item, null);
                        if (propertyValue != null && (propertyValue is double || propertyValue is int || propertyValue is float))
                        {                            
                            double value = Convert.ToDouble(propertyValue);
                            if (value < (4/8))
                            {
                                color = "Red";
                            }
                            else if (value >= (4/8) && value < (20/8))
                            {
                                color = "Yellow";
                            }
                            else if (value >= (20/8) && value < (50/8))
                            {
                                color = "Blue";
                            }
                            else
                            {
                                color = "Green";
                            }

                            colorAssignments.Add((id, color));
                        }

                        else
                        {
                            colorAssignments.Add((id, null));
                        }
                    }
                }
            }

            return colorAssignments;
        }

        private List<(int Id, string Color)> AssignColors(List<BaseCsvData> list, string dataSelection, Dictionary<string, List<MapColorIntervals>> dataIntervals)
        {
            List<(int Id, string Color)> colorAssignments = new List<(int Id, string Color)>();

            if (list.Count > 0 && dataIntervals.ContainsKey(dataSelection))
            {
                var intervals = dataIntervals[dataSelection];

                for (int i = 0; i < list.Count; i++)
                {
                    var item = list[i];
                    var propertyInfo = typeof(BaseCsvData).GetProperty(dataSelection);

                    if (propertyInfo != null)
                    {
                        int id = i;
                        string color = null;

                        object propertyValue = propertyInfo.GetValue(item, null);

                        if (propertyValue != null && (propertyValue is double || propertyValue is int || propertyValue is float))
                        {
                            double value = Convert.ToDouble(propertyValue);

                            foreach (var interval in intervals)
                            {
                                if (value >= interval.LowerLimit && value < interval.UpperLimit)
                                {
                                    color = interval.Color;
                                    break;
                                }
                            }
                        }

                        colorAssignments.Add((id, color));
                    }
                }
            }

            return colorAssignments;
        }

        private string KMLGenerator(List<BaseCsvData> list, string dataSelection)
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

        private void InfoCardText()
        {
            if(tabSelector == "Ping")
            {
                greenCard.Title = "Minimum";
                redCard.Title = "Maximum";
            }
            else
            {
                greenCard.Title = "Maximum";
                redCard.Title = "Minimum";
            }
        }

        private void ChangeButtonStyle(string buttonName)
        {
            // postavi sve u početni stil
            rsrpButton.Style = (Style)FindResource("menuButton");
            rsrqButton.Style = (Style)FindResource("menuButton");
            sinrButton.Style = (Style)FindResource("menuButton");
            cqiButton.Style = (Style)FindResource("menuButton");
            pciButton.Style = (Style)FindResource("menuButton");
            pingButton.Style = (Style)FindResource("menuButton");
            downButton.Style = (Style)FindResource("menuButton");

            // postavi stil aktivnog
            switch (buttonName)
            {
                case "RSRP":
                    rsrpButton.Style = (Style)FindResource("menuButtonActive");
                    break;
                case "RSRQ":
                    rsrqButton.Style = (Style)FindResource("menuButtonActive");
                    break;
                case "SINR":
                    sinrButton.Style = (Style)FindResource("menuButtonActive");
                    break;
                case "CQI":
                    cqiButton.Style = (Style)FindResource("menuButtonActive");
                    break;
                case "PCI":
                    pciButton.Style = (Style)FindResource("menuButtonActive");
                    break;
                case "Ping":
                    pingButton.Style = (Style)FindResource("menuButtonActive");
                    break;
                case "Downlink":
                    downButton.Style = (Style)FindResource("menuButtonActive");
                    break;
                default:                    
                    break;
            }
        }

        private void ToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            UpdateGraph();
        }

        private void ToggleButton_Unchecked(object sender, RoutedEventArgs e)
        {
            UpdateGraph();
        }

    }
    
}
