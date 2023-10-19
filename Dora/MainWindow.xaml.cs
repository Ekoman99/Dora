using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using CsvHelper;
using CsvHelper.Configuration;
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
        }        
        
        public string FilePath
        {           
            get { return (string)GetValue(filePathCSVProperty); }
            set { SetValue(filePathCSVProperty, value); }
        }

        public static readonly DependencyProperty filePathCSVProperty =
            DependencyProperty.Register("FilePath", typeof(string), typeof(MainWindow), new PropertyMetadata(string.Empty));

        public void CSV_File_Selection(object sender, RoutedEventArgs e)
        {
            bool dataLoadedCSV = false;

            try
            {
                Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
                dlg.Filter = "CSV Files (*.csv)|*.csv"; //filter za prikaz samo.csv
                Nullable<bool> result = dlg.ShowDialog();

                if (result == true)
                {
                    FilePath = dlg.FileName;
                    dataLoadedCSV = true;
                }

                List<BaseCsvData> inputDataList = LoadDataFromCsv(FilePath);
                /* dataListView.ItemsSource = csvDataList; za staru verziju i prikaz liste test */

                if (dataLoadedCSV == true)
                {
                    //initial screen showing RSRP
                    ShowScreen(inputDataList, "RSRP");
                    InsertGraph(inputDataList, "RSRP");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
                FilePath = string.Empty; // filepath ostaje prazan
            }
            
        }
        
        public List<BaseCsvData> LoadDataFromCsv(string filePath)
        {
            List<BaseCsvData> dataList = new List<BaseCsvData>();

            // csv file reader storing into BaseCsvData object list
            var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture);
            csvConfig.Delimiter = ";";
            csvConfig.HasHeaderRecord = true; // csv header.

            using (var reader = new StreamReader(filePath))
            using (var csv = new CsvReader(reader, csvConfig))
            {
                dataList = csv.GetRecords<BaseCsvData>().ToList();                
            }

            return dataList;
        }

        private void MapShow_Click(object sender, RoutedEventArgs e)
        {

            var coordinates = new List<(double Latitude, double Longitude)>
            {
                (45.772042325977885, 15.98029954968336),
                (45.77367361725392, 15.975119210105513),
                // Add more coordinates as needed
            };

            var mapWindow = new RouteWindow(coordinates);
            mapWindow.Show();
        }

        private double CalculateAverage(List<BaseCsvData> list, string propertyName)
        {
            double average = 0;

            if(list.Count > 0)
            {
                average = list.Average(item =>
                {
                    var propertyInfo = typeof(BaseCsvData).GetProperty(propertyName);
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

            return average;
        }

        private double CalculateMinimum(List<BaseCsvData> list, string propertyName)
        {
            double average = 0;

            if (list.Count > 0)
            {
                average = list.Min(item =>
                {
                    var propertyInfo = typeof(BaseCsvData).GetProperty(propertyName);
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

            return average;
        }

        private double CalculateMaximum (List<BaseCsvData> list, string propertyName)
        {
            double average = 0;

            if (list.Count > 0)
            {
                average = list.Max(item =>
                {
                    var propertyInfo = typeof(BaseCsvData).GetProperty(propertyName);
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

            return average;
        }

        private void ShowScreen(List<BaseCsvData> inputDataList, string dataSelection)
        {
            double minimumValue = CalculateMinimum(inputDataList, dataSelection);
            rsrpMax.Number = minimumValue.ToString() + "dBm";
            double maximumValue = CalculateMaximum(inputDataList, dataSelection);
            rsrpMin.Number = maximumValue.ToString() + "dBm";
            double averageValue = CalculateAverage(inputDataList, dataSelection);
            rsrpAverage.Number = averageValue.ToString("n2") + "dBm";
            /* MessageBox.Show("min:" + minimumValue + "\nmax:" + maximumValue + "\navg:" + averageValue); --> samo za test podataka */

            List<DateTime> listTime = new List<DateTime>();
            List<float> listRSRP = new List<float>();


            for (int i = 0; i < inputDataList.Count; i++)
            {
                listTime.Add(inputDataList[i].Time);
                listRSRP.Add((float)inputDataList[i].RSRP);
            }

            /*var chartValues = listRSRP.AsChartValues();
            var lineSeries = new LineSeries
            {
                Values = chartValues
            };*/
        }

        private void InsertGraph (List<BaseCsvData> inputList, string dataSelection)
        {
            // Create a new PlotModel
            var model = new PlotModel();

            // Create an example series (you can modify this to fit your data)
            var series = new LineSeries();
            series.Points.Add(new DataPoint(0, 0));
            series.Points.Add(new DataPoint(1, 1));
            series.Points.Add(new DataPoint(2, 0));
            series.Points.Add(new DataPoint(3, 2));

            // Add the series to the model
            model.Series.Add(series);

            // Create axes (if needed)
            var xAxis = new LinearAxis { Position = AxisPosition.Bottom };
            var yAxis = new LinearAxis { Position = AxisPosition.Left };

            // Add axes to the model (if needed)
            model.Axes.Add(xAxis);
            model.Axes.Add(yAxis);

            // Replace the chart container with the OxyPlot chart
            var oxyplotChart = new OxyPlot.Wpf.PlotView
            {
                Model = model,
        };

            oxyplotChartContainer.Children.Clear();
            oxyplotChartContainer.Children.Add(oxyplotChart);
        }
    }
    
}
