using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
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

        List<BaseCsvData> inputDataList;
        List<(double Latitude, double Longitude)> mainGeoList;

        private bool status4G;
        private bool status5G;
        private bool loadComplete = false;

        public void CSVFileSelect(object sender, RoutedEventArgs e)
        {
            bool dataLoadedCSV = false;

            try
            {
                Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
                dlg.Filter = "CSV Files (*.csv)|*.csv"; // filter za prikaz samo.csv
                Nullable<bool> result = dlg.ShowDialog();

                if (result == true)
                {
                    FilePath = dlg.FileName;
                    dataLoadedCSV = true;
                    loadComplete = true;
                }

                inputDataList = LoadCSV(FilePath);

                status4G = Check4G(inputDataList);
                status5G = Check5G(inputDataList);

                mainGeoList = GetCoordinates(inputDataList);

                if (dataLoadedCSV == true)
                {
                    // incijalno pokazivanje RSRP
                    ShowScreen(inputDataList, "RSRP");
                    InsertGraph(inputDataList, "RSRP");
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
                    throw new Exception ("errror");
                }
            }
        }

        private void ShowMap(object sender, RoutedEventArgs e)
        {

            /*var coordinates = new List<(double Latitude, double Longitude)>
            {
                (45.772042325977885, 15.98029954968336),
                (45.77367361725392, 15.975119210105513),
                // Add more coordinates as needed
            };*/

            if (loadComplete == false)
            {
                var warningWindow = new UnloadedWarning();
                warningWindow.Show();
            }
            else
            {
                var mapWindow = new RouteWindow(mainGeoList);
                mapWindow.Show();
            }
            
        }

        private void ClickRSRP(object sender, RoutedEventArgs e)
        {
            string dataSelection = "RSRP";

            double minimumValue = CalculateMaximum(inputDataList, dataSelection);
            rsrpMax.Number = minimumValue.ToString() + "dBm";
            double maximumValue = CalculateMinimum(inputDataList, dataSelection);
            rsrpMin.Number = maximumValue.ToString() + "dBm";
            double averageValue = CalculateAverage(inputDataList, dataSelection);
            rsrpAverage.Number = averageValue.ToString("n2") + "dBm";
            /* MessageBox.Show("min:" + minimumValue + "\nmax:" + maximumValue + "\navg:" + averageValue); --> samo za test podataka */

            List<DateTime> listTime = new List<DateTime>();
            List<float> listRSRP = new List<float>();


            for (int i = 0; i < inputDataList.Count; i++)
            {
                listTime.Add(inputDataList[i].Time);
                listRSRP.Add((float)inputDataList[i].RSRQ);
            }

            InsertGraph(inputDataList, "RSRP");
        }

        private void ClickRSRQ(object sender, RoutedEventArgs e)
        {
            string dataSelection = "RSRQ";

            double minimumValue = CalculateMaximum(inputDataList, dataSelection);
            rsrpMax.Number = minimumValue.ToString() + "dB";
            double maximumValue = CalculateMinimum(inputDataList, dataSelection);
            rsrpMin.Number = maximumValue.ToString() + "dB";
            double averageValue = CalculateAverage(inputDataList, dataSelection);
            rsrpAverage.Number = averageValue.ToString("n2") + "dB";
            /* MessageBox.Show("min:" + minimumValue + "\nmax:" + maximumValue + "\navg:" + averageValue); --> samo za test podataka */

            List<DateTime> listTime = new List<DateTime>();
            List<float> listRSRP = new List<float>();


            for (int i = 0; i < inputDataList.Count; i++)
            {
                listTime.Add(inputDataList[i].Time);
                listRSRP.Add((float)inputDataList[i].RSRQ);
            }

            InsertGraph(inputDataList, "RSRQ");
        }

        private void ClickSINR(object sender, RoutedEventArgs e)
        {
            string dataSelection = "SINR";

            double minimumValue = CalculateMaximum(inputDataList, dataSelection);
            rsrpMax.Number = minimumValue.ToString() + "dB";
            double maximumValue = CalculateMinimum(inputDataList, dataSelection);
            rsrpMin.Number = maximumValue.ToString() + "dB";
            double averageValue = CalculateAverage(inputDataList, dataSelection);
            rsrpAverage.Number = averageValue.ToString("n2") + "dB";
            /* MessageBox.Show("min:" + minimumValue + "\nmax:" + maximumValue + "\navg:" + averageValue); --> samo za test podataka */

            List<DateTime> listTime = new List<DateTime>();
            List<float> listRSRP = new List<float>();


            for (int i = 0; i < inputDataList.Count; i++)
            {
                listTime.Add(inputDataList[i].Time);
                listRSRP.Add((float)inputDataList[i].RSRQ);
            }

            InsertGraph(inputDataList, "SINR");
        }

        private void ClickCQI(object sender, RoutedEventArgs e)
        {
            string dataSelection = "CQI";

            double minimumValue = CalculateMaximum(inputDataList, dataSelection);
            rsrpMax.Number = minimumValue.ToString() + "";
            double maximumValue = CalculateMinimum(inputDataList, dataSelection);
            rsrpMin.Number = maximumValue.ToString() + "";
            double averageValue = CalculateAverage(inputDataList, dataSelection);
            rsrpAverage.Number = averageValue.ToString("n2") + "";
            /* MessageBox.Show("min:" + minimumValue + "\nmax:" + maximumValue + "\navg:" + averageValue); --> samo za test podataka */

            List<DateTime> listTime = new List<DateTime>();
            List<float> listRSRP = new List<float>();


            for (int i = 0; i < inputDataList.Count; i++)
            {
                listTime.Add(inputDataList[i].Time);
                listRSRP.Add((float)inputDataList[i].RSRQ);
            }

            InsertGraph(inputDataList, "CQI");
        }

        private void ClickPing(object sender, RoutedEventArgs e)
        {
            string dataSelection = "Ping";

            double minimumValue = CalculateMaximum(inputDataList, dataSelection);
            rsrpMax.Number = minimumValue.ToString() + "ms";
            double maximumValue = CalculateMinimum(inputDataList, dataSelection);
            rsrpMin.Number = maximumValue.ToString() + "ms";
            double averageValue = CalculateAverage(inputDataList, dataSelection);
            rsrpAverage.Number = averageValue.ToString("n2") + "ms";
            /* MessageBox.Show("min:" + minimumValue + "\nmax:" + maximumValue + "\navg:" + averageValue); --> samo za test podataka */

            List<DateTime> listTime = new List<DateTime>();
            List<float> listRSRP = new List<float>();


            for (int i = 0; i < inputDataList.Count; i++)
            {
                listTime.Add(inputDataList[i].Time);
                listRSRP.Add((float)inputDataList[i].RSRQ);
            }

            InsertGraph(inputDataList, "Ping");
        }

        private void ClickThroughput(object sender, RoutedEventArgs e)
        {
            string dataSelection = "Downlink";

            double minimumValue = CalculateMaximum(inputDataList, dataSelection);
            rsrpMax.Number = (minimumValue*8).ToString("n2") + "Mbps";
            double maximumValue = CalculateMinimum(inputDataList, dataSelection);
            rsrpMin.Number = (maximumValue*8).ToString("n2") + "Mbps";
            double averageValue = CalculateAverage(inputDataList, dataSelection);
            rsrpAverage.Number = (averageValue*8).ToString("n2") + "Mbps";
            /* MessageBox.Show("min:" + minimumValue + "\nmax:" + maximumValue + "\navg:" + averageValue); --> samo za test podataka */

            List<DateTime> listTime = new List<DateTime>();
            List<float> listRSRP = new List<float>();


            for (int i = 0; i < inputDataList.Count; i++)
            {
                listTime.Add(inputDataList[i].Time);
                listRSRP.Add((float)inputDataList[i].RSRQ);
            }

            InsertGraph(inputDataList, "Downlink");
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

        private void ShowScreen(List<BaseCsvData> inputDataList, string dataSelection)
        {
            double minimumValue = CalculateMaximum(inputDataList, dataSelection);
            rsrpMax.Number = minimumValue.ToString() + "dBm";
            double maximumValue = CalculateMinimum(inputDataList, dataSelection);
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
            // kreiranje modela za plotanje
            var model = new PlotModel
            {
                Background = OxyColors.Transparent,
                PlotAreaBorderColor = OxyColors.Transparent,
            };

            // serija točaka
            var series = new LineSeries
            {
                Color = OxyColors.White,
            };

            for (int i = 0; i < inputDataList.Count; i++)
            {
                // uzimanje vrijednosti, dataSelection definira koji property 
                object dataValue = inputDataList[i].GetType().GetProperty(dataSelection).GetValue(inputDataList[i]);

                series.Points.Add(new DataPoint(DateTimeAxis.ToDouble(inputDataList[i].Time), Convert.ToDouble(dataValue)));
            }

            model.Series.Add(series);

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

        private void Logoff(object sender, RoutedEventArgs e)
        {
            var login = new Login();
            login.Show();
            this.Close();
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


    }
    
}
