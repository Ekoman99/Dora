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
using System.Windows.Controls;
using System.IO;
using OxyPlot;
using Dora.Data;
using Newtonsoft.Json;
using Dora.UI;
using System.Runtime.InteropServices.ComTypes;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using OxyPlot.Wpf;
using Dora.Models;

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

        public string filePath;

        List<BaseCsvData> inputDataList;
        List<(double Latitude, double Longitude)> mainGeoList;

        GraphConfig graphConfig = new GraphConfig();

        SettingsDefinitions allSettings;
        Dictionary<string, List<MapColorIntervals>> dataIntervals;
        Dictionary<string, string> lastSavedPaths = new Dictionary<string, string>(); // pohrana zadnjeg patha za različite tipove datoteka, implementirano za .csv i .png

        InfoCard greenCard = InfoCard.GreenCardDefault;
        InfoCard blueCard = InfoCard.BlueCardDefault;
        InfoCard redCard = InfoCard.RedCardDefault;

        private bool loadComplete = false;
        bool peakSmooth = true;
        int peakUpperLimit = 50000;
        string tabSelector = "RSRP"; //program prvo učita RSRP

        private PlotModel model; //model mora biti dostupan klasi zbog interakcije metoda grafa i exportera

        private bool isLineOptionSelected;
        private bool isInterpolationEnabled;
        private int interpolationValue = 1;

        public bool IsLineOptionSelected
        {
            get { return isLineOptionSelected; }
            set
            {
                if (isLineOptionSelected != value)
                {
                    isLineOptionSelected = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsInterpolationEnabled
        {
            get { return isInterpolationEnabled; }
            set
            {
                if (isInterpolationEnabled != value)
                {
                    isInterpolationEnabled = value;
                    OnPropertyChanged();
                    UpdateGraph();
                }
            }
        }

        public int InterpolationValue
        {
            get { return interpolationValue; }
            set
            {
                if (interpolationValue != value)
                {
                    interpolationValue = value;
                    OnPropertyChanged();
                    UpdateGraph();
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
                    filePath = openCSVDialog.FileName;
                    dataLoadedCSV = true;
                    loadComplete = true;
                    lastSavedPaths[".csv"] = Path.GetDirectoryName(openCSVDialog.FileName);
                }

                CSVHandler csvHandler = new CSVHandler();
                inputDataList = csvHandler.LoadCSV(filePath);

                mainGeoList = GetCoordinates(inputDataList);

                if (dataLoadedCSV == true)
                {
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
                filePath = string.Empty; // filepath ostaje prazan
            }

        }

        public class NullableIntTypeConverter : DefaultTypeConverter
        {
            public override object ConvertFromString(string text, IReaderRow row, MemberMapData memberMapData) //override standardnog konvertera za format podataka
            {
                if (string.IsNullOrWhiteSpace(text) || text.Equals("N/A", StringComparison.OrdinalIgnoreCase))
                {
                    return null;
                }

                if (int.TryParse(text, out int result))
                {
                    return result;
                }
                else
                {
                    throw new Exception("error");
                }
            }
        }

        private void InitializeMapIntervals()
        {
            string folderPath = FindSettingsDirectory();
            string filePath = Path.Combine(folderPath, "Settings", "MapIntervals.json");
            string json = File.ReadAllText(filePath);

            dataIntervals = JsonConvert.DeserializeObject<Dictionary<string, List<MapColorIntervals>>>(json);
        }

        private void showInfo(object sender, RoutedEventArgs e)
        {
            var infoWindow = new InfoWindow();
            infoWindow.Show();
        }

        private void InitializeSettings()
        {
            string folderPath = FindSettingsDirectory();
            string filePath = Path.Combine(folderPath, "Settings", "Settings.json");
            string json = File.ReadAllText(filePath);

            allSettings = JsonConvert.DeserializeObject<SettingsDefinitions>(json);
            FillGraphConfig();
        }

        private void FillGraphConfig()
        {
            graphConfig.PeakLimit = peakUpperLimit;
            graphConfig.PeakNormalization = peakSmooth;
            graphConfig.NRcolor = allSettings.NRColor;
            graphConfig.LTEcolor = allSettings.LTEColor;
            graphConfig.GraphBackground = allSettings.GraphBackground;
            graphConfig.GraphElements = allSettings.GraphElements;
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
                    List<(int Id, string Color)> boje = Charting.AssignColors(inputDataList, tabSelector, dataIntervals);

                    var mapWindow = new RouteWindow(mainGeoList, boje);
                    mapWindow.Show();
                }

                else
                {
                    var mapWindow = new RouteWindow(mainGeoList);
                    mapWindow.Show();
                }
            }

        }

        public void ExportGraph(object sender, RoutedEventArgs e)
        {
            Exporter.ExportGraph(loadComplete, model, lastSavedPaths, allSettings);
        }

        private void ExportKML(object sender, RoutedEventArgs e)
        {
            if (loadComplete)
            {
                Exporter.ExportKmlFile(inputDataList, allSettings);
            }
            else
            {
                var warningWindow = new UnloadedWarning();
                warningWindow.Show();
            }
        }

        private void MapIntervalEditor(object sender, RoutedEventArgs e)
        {
            string folderPath = FindSettingsDirectory();
            string settingsPath = Path.Combine(folderPath, "Settings", "MapIntervals.json");

            var editorWindow = new SettingsWindow(dataIntervals, settingsPath);
            editorWindow.ShowDialog();

            // reload mape
            InitializeMapIntervals();
        }

        private void OpenColorSettings(object sender, RoutedEventArgs e)
        {
            string folderPath = FindSettingsDirectory();
            string settingsPath = Path.Combine(folderPath, "Settings", "Settings.json");

            var colorWindow = new ColorSettingsWindow(allSettings, settingsPath);
            colorWindow.ShowDialog();

            // reload nakon
            InitializeSettings();

            if (loadComplete)
            {
                UpdateGraph();
            }
        }

        private void UniversalClick(object sender, RoutedEventArgs e, string dataSelection, string unit)
        {
            if (loadComplete == true)
            {
                chartTitle.Text = dataSelection;

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
            UniversalClick(sender, e, "RSRP", "dBm");
        }

        private void ClickRSRQ(object sender, RoutedEventArgs e)
        {
            UniversalClick(sender, e, "RSRQ", "dB");
        }

        private void ClickSINR(object sender, RoutedEventArgs e)
        {
            UniversalClick(sender, e, "SINR", "dB");
        }

        private void ClickCQI(object sender, RoutedEventArgs e)
        {
            UniversalClick(sender, e, "CQI", "");
        }

        private void ClickPCI(object sender, RoutedEventArgs e)
        {
            UniversalClick(sender, e, "PCI", "");
        }

        private void ClickPing(object sender, RoutedEventArgs e)
        {
            UniversalClick(sender, e, "Ping", "ms");
        }

        private void ClickThroughput(object sender, RoutedEventArgs e)
        {
            UniversalClick(sender, e, "Downlink", "MBps");
        }

        private void Logoff(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        #endregion

        private void CalculateCards(string dataSelection, string unit, bool peakSmooth, int peakUpperLimit)
        {
            var (greenValue, redValue, blueValue) = MathEngine.CalculateCardValues(inputDataList, dataSelection, unit, peakSmooth, peakUpperLimit);

            greenCard.Number = greenValue;
            redCard.Number = redValue;
            blueCard.Number = blueValue;
        }

        private void UpdateGraph()
        {
            if (loadComplete == true)
            {
                if(!isLineOptionSelected)
                {
                    var oxyplotChart = VisualisationEngine.StemGraph(inputDataList, tabSelector, graphConfig, interpolationValue, isInterpolationEnabled); // ako je toggle off
                    model = oxyplotChart.Model;
                    oxyplotChartContainer.Children.Clear();
                    oxyplotChartContainer.Children.Add(oxyplotChart);
                }
                else if (isLineOptionSelected)
                {
                    var oxyplotChart = VisualisationEngine.LineGraph(inputDataList, tabSelector, graphConfig); // ako je toggle on
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

        /*private List<(int Id, string Color)> AssignColors(List<BaseCsvData> list, string dataSelection, Dictionary<string, List<MapColorIntervals>> colorIntervals)
        {
            List<(int Id, string Color)> colorList = new List<(int Id, string Color)>();

            if (!colorIntervals.ContainsKey(dataSelection))
            {
                return colorList;
            }

            var propertyInfo = typeof(BaseCsvData).GetProperty(dataSelection);
            if (propertyInfo == null)
            {
                return colorList;
            }

            var intervals = colorIntervals[dataSelection];

            for (int i = 0; i < list.Count; i++)
            {
                var item = list[i];
                var value = propertyInfo.GetValue(item);
                string color = null;

                if (value != null && (value is double || value is int || value is float))
                {
                    double numberValue = Convert.ToDouble(value);

                    foreach (var interval in intervals)
                    {
                        if (numberValue >= interval.LowerLimit && numberValue < interval.UpperLimit)
                        {
                            color = interval.Color;
                            break;
                        }
                    }
                }

                colorList.Add((i, color));
            }

            return colorList;
        }*/

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
