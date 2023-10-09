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
using LiveCharts.Wpf;
using LiveCharts.Helpers;
using LiveCharts;

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

        public SeriesCollection SeriesCollection { get; set; }

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

                List<CsvData> inputDataList = LoadDataFromCsv(FilePath);
                /*dataListView.ItemsSource = csvDataList; za staru verziju i prikaz liste test*/

                if (dataLoadedCSV == true)
                {
                    // Create an ObservableCollection to store the data.
                    ObservableCollection<float> rsrpCollection = new ObservableCollection<float>();

                    // Copy data from the list to the ObservableCollection.
                    foreach (var item in inputDataList)
                    {
                        rsrpCollection.Add(item.RSRP);
                    }

                    float minimumValue = rsrpCollection.Min();
                    rsrpMax.Number = minimumValue.ToString() + "dBm";
                    float maximumValue = rsrpCollection.Max();
                    rsrpMin.Number = maximumValue.ToString() + "dBm";
                    float averageValue = rsrpCollection.Average();
                    rsrpAverage.Number = averageValue.ToString("n2") + "dBm";
                    MessageBox.Show("min:" + minimumValue + "\nmax:" + maximumValue + "\navg:" + averageValue);

                    /*var lineSeries = new LineSeries
                    {
                        Values = inputDataList.AsChartValues()
                    };*/

                    List<DateTime> listTime = new List<DateTime>();
                    List<float> listRSRP = new List<float>();

                    foreach (var item in inputDataList)
                    {
                        listTime.Add(item.Time);
                        listRSRP.Add(item.RSRP);
                    }

                    var chartValues = listRSRP.AsChartValues();
                    var lineSeries = new LineSeries
                    {
                        Values = chartValues
                    };

                    cartesianChart.Values = chartValues;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
                FilePath = string.Empty; // filepath ostaje prazan
            }
            
        }
        public class CsvData
        {
            [Name(" RSRP [dBm]")]
            public float RSRP { get; set; }

            [Name("Time [hh:mm:ss]")]
            public DateTime Time { get; set; }
        }

        public List<CsvData> LoadDataFromCsv(string filePath)
        {
            List<CsvData> dataList = new List<CsvData>();

            // csv file reader
            var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture);
            csvConfig.Delimiter = ";";
            csvConfig.HasHeaderRecord = true; // csv header.

            using (var reader = new StreamReader(filePath))
            using (var csv = new CsvReader(reader, csvConfig))
            {
                dataList = csv.GetRecords<CsvData>().ToList();
            }

            return dataList;
        }

        
    }
}
