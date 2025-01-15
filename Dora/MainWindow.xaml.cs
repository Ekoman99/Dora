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
using OxyPlot.Wpf;

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

            InitializeSettings();
            InitializeMapIntervals();

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

        SettingsDefinitions AllSettings;
        Dictionary<string, List<MapColorIntervals>> DataIntervals;
        Dictionary<string, string> lastSavedPaths = new Dictionary<string, string>(); //pohrana zadnjeg patha za različite tipove datoteka, implementirano za .csv i .png

        InfoCard greenCard = InfoCard.GreenCardDefault;
        InfoCard blueCard = InfoCard.BlueCardDefault;
        InfoCard redCard = InfoCard.RedCardDefault;
        WideInfoCard wideCard = WideInfoCard.WideCardDefault;

        private bool status4G;
        private bool status5G;
        private bool loadComplete = false;
        bool peakSmooth = true;
        int peakUpperLimit = 50000;
        string tabSelector = "RSRP"; //program prvo učita RSRP

        private PlotModel model; //model mora biti dostupan klasi zbog interakcije metoda grafa i exportera

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
                    lastSavedPaths[".csv"] = Path.GetDirectoryName(openCSVDialog.FileName);
                }
                
                CSVHandler csvHandler = new CSVHandler();
                inputDataList = csvHandler.LoadCSV(FilePath);

                status4G = Check4G(inputDataList);
                status5G = Check5G(inputDataList);

                MainGeoList = GetCoordinates(inputDataList);

                if (dataLoadedCSV == true)
                {
                    // incijalno pokazivanje RSRP
                    //ShowScreen(inputDataList, "RSRP"); //pokazuje inicijalne izračune
                    //LineGraph(inputDataList, "RSRP");

                    tabSelector = "RSRP";
                    CalculateCards(tabSelector, "dBm", peakSmooth, peakUpperLimit);
                    UpdateGraph();

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

        /*private Dictionary<string, List<MapColorIntervals>> InitializeMapIntervals()
        {
            //string filePath = @"C:\Users\Josip\source\repos\Dora\Dora\Data\MapIntervals.json";
            string folderPath = FindSettingsDirectory();
            string filePath = Path.Combine(folderPath, "Settings", "MapIntervals.json");
            string json = File.ReadAllText(filePath);

            Dictionary<string, List<MapColorIntervals>> dataIntervals = JsonConvert.DeserializeObject<Dictionary<string, List<MapColorIntervals>>>(json);

            return dataIntervals;
        }*/

        private void InitializeMapIntervals()
        {
            string folderPath = FindSettingsDirectory();
            string filePath = Path.Combine(folderPath, "Settings", "MapIntervals.json");
            string json = File.ReadAllText(filePath);

            DataIntervals = JsonConvert.DeserializeObject<Dictionary<string, List<MapColorIntervals>>>(json);
        }

        private void testwide(object sender, RoutedEventArgs e)
        {
            infoCardGrid.Children.Remove(greenCard);
            infoCardGrid.Children.Remove(blueCard);
            infoCardGrid.Children.Remove(redCard);

            infoCardGrid.Children.Add(wideCard);
            Grid.SetColumnSpan(wideCard, 3);
        }

        private void InitializeSettings()
        {
            string folderPath = FindSettingsDirectory();
            string filePath = Path.Combine(folderPath, "Settings", "Settings.json");
            string json = File.ReadAllText(filePath);

            AllSettings = JsonConvert.DeserializeObject<SettingsDefinitions>(json);
        }

        private string FindSettingsDirectory()
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            while (directoryInfo != null && !directoryInfo.GetDirectories().Any(dir => dir.Name == "Settings"))
            {
                directoryInfo = directoryInfo.Parent;
            }

            return directoryInfo.FullName;
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
            Exporter.ExportGraph(loadComplete, model, lastSavedPaths, AllSettings);
        }

        private void ExportKML(object sender, RoutedEventArgs e)
        {
            if (loadComplete)
            {
                Exporter.ExportKmlFile(inputDataList, AllSettings);
            }
            else
            {
                var warningWindow = new UnloadedWarning();
                warningWindow.Show();
            }
        }

        private void ClickHandler(object sender, RoutedEventArgs e, string dataSelection, string unit)
        {
            if (loadComplete == true)
            {
                chartTitle.Text = dataSelection;

                // You can calculate the cards or other specific actions as needed.
                tabSelector = dataSelection;
                CalculateCards(dataSelection, unit, peakSmooth, peakUpperLimit);
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

        #region button click methods

        private void ClickRSRP(object sender, RoutedEventArgs e)
        {
            ClickHandler(sender, e, "RSRP", "dBm");
        }

        private void ClickRSRQ(object sender, RoutedEventArgs e)
        {
            ClickHandler(sender, e, "RSRQ", "dB");
        }

        private void ClickSINR(object sender, RoutedEventArgs e)
        {
            ClickHandler(sender, e, "SINR", "dB");
        }

        private void ClickCQI(object sender, RoutedEventArgs e)
        {
            ClickHandler(sender, e, "CQI", "");
        }

        private void ClickPCI(object sender, RoutedEventArgs e)
        {
            ClickHandler(sender, e, "PCI", "");
        }

        private void ClickPing(object sender, RoutedEventArgs e)
        {
            ClickHandler(sender, e, "Ping", "ms");
        }

        private void ClickThroughput(object sender, RoutedEventArgs e)
        {
            ClickHandler(sender, e, "Downlink", "Mbps");
        }

        private void Logoff(object sender, RoutedEventArgs e)
        {
            var login = new Login();
            login.Show();
            this.Close();
        }

        #endregion

        private void CalculateCards(string dataSelection, string unit, bool peakSmooth, int peakUpperLimit)
        {
            if(dataSelection == "Ping")
            {
                greenCard.Number = MathEngine.CalculateMinimum(inputDataList, dataSelection).ToString() + unit;
                redCard.Number = MathEngine.CalculateMaximum(inputDataList, dataSelection).ToString() + unit;
                switch (dataSelection)
                {
                    case "CQI":
                        {
                            blueCard.Number = Math.Floor(MathEngine.CalculateAverage(inputDataList, dataSelection, peakSmooth, peakUpperLimit)).ToString("n2") + unit;
                            break;
                        }
                    case "PCI":
                        {
                            blueCard.Number = "N/A";
                            break;
                        }
                    default:
                        {
                            blueCard.Number = MathEngine.CalculateAverage(inputDataList, dataSelection, peakSmooth, peakUpperLimit).ToString("n2") + unit;
                            break;
                        }
                }
            }
            else
            {
                greenCard.Number = MathEngine.CalculateMaximum(inputDataList, dataSelection, peakSmooth, peakUpperLimit).ToString("n2") + unit;
                redCard.Number = MathEngine.CalculateMinimum(inputDataList, dataSelection).ToString("n2") + unit;
                switch (dataSelection)
                {
                    case "CQI":
                        {
                            blueCard.Number = Math.Floor(MathEngine.CalculateAverage(inputDataList, dataSelection, peakSmooth, peakUpperLimit)).ToString("n2") + unit;
                            break;
                        }
                    case "PCI":
                        {
                            blueCard.Number = "N/A";
                            break;
                        }
                    default:
                        {
                            blueCard.Number = MathEngine.CalculateAverage(inputDataList, dataSelection, peakSmooth, peakUpperLimit).ToString("n2") + unit;
                            break;
                        }
                }
            }
            
        }

        private void UpdateGraph()
        {
            if(loadComplete == true)
            {
                if (IsOption1Selected && (tabSelector == "RSRQ" || tabSelector == "SINR"))
                {
                    var oxyplotChart = VisualisationEngine.LineGraph(inputDataList, tabSelector, peakSmooth, peakUpperLimit); // Execute LineGraph method if toggle button is on
                    model = oxyplotChart.Model;
                    oxyplotChartContainer.Children.Clear();
                    oxyplotChartContainer.Children.Add(oxyplotChart);
                }
                else if (IsOption1Selected && !(tabSelector == "RSRQ" || tabSelector == "SINR"))
                {
                    var oxyplotChart = VisualisationEngine.LineGraph(inputDataList, tabSelector);
                    model = oxyplotChart.Model;
                    oxyplotChartContainer.Children.Clear();
                    oxyplotChartContainer.Children.Add(oxyplotChart);
                }
                else if (!IsOption1Selected && (tabSelector == "RSRQ" || tabSelector == "SINR"))
                {
                    var oxyplotChart = VisualisationEngine.StemGraph(inputDataList, tabSelector, peakSmooth, peakUpperLimit); // Execute StemGraph method if toggle button is off
                    model = oxyplotChart.Model;
                    oxyplotChartContainer.Children.Clear();
                    oxyplotChartContainer.Children.Add(oxyplotChart);
                }
                else
                {
                    var oxyplotChart = VisualisationEngine.StemGraph(inputDataList, tabSelector);
                    model = oxyplotChart.Model;
                    oxyplotChartContainer.Children.Clear();
                    oxyplotChartContainer.Children.Add(oxyplotChart);
                }
            }
            else
            {
                var warningWindow = new UnloadedWarning();
                warningWindow.Show();
            }
        }  

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
