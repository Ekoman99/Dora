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
        Dictionary<string, List<MapColorIntervals>> dataIntervals;

        private bool status4G;
        private bool status5G;
        private bool loadComplete = false;
        bool peakSmooth = true;
        int peakUpperLimit = 50000;
        string tabSelector = "RSRP"; //program prvo učita RSRP

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
                if (tabSelector == "Downlink")
                {
                    List<(int Id, string Color)> boje = AssignColors(inputDataList, "Downlink");

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

        private void ClickRSRP(object sender, RoutedEventArgs e)
        {
            if (loadComplete == true)
            {
                string dataSelection = "RSRP";
                string unit = "dBm";
                chartTitle.Text = dataSelection;

                double minimumValue = CalculateMaximum(inputDataList, dataSelection);
                rsrpMax.Number = minimumValue.ToString() + unit;
                double maximumValue = CalculateMinimum(inputDataList, dataSelection);
                rsrpMin.Number = maximumValue.ToString() + unit;
                double averageValue = CalculateAverage(inputDataList, dataSelection);
                rsrpAverage.Number = averageValue.ToString("n2") + unit;
                /* MessageBox.Show("min:" + minimumValue + "\nmax:" + maximumValue + "\navg:" + averageValue); --> samo za test podataka */

                List<DateTime> listTime = new List<DateTime>();
                List<float> listRSRP = new List<float>();


                for (int i = 0; i < inputDataList.Count; i++)
                {
                    listTime.Add(inputDataList[i].Time);
                    listRSRP.Add((float)inputDataList[i].RSRQ);
                }

                InsertGraph(inputDataList, "RSRP");
                tabSelector = "RSRP";
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
                rsrpMax.Number = minimumValue.ToString() + unit;
                double maximumValue = CalculateMinimum(inputDataList, dataSelection);
                rsrpMin.Number = maximumValue.ToString() + unit;
                double averageValue = CalculateAverage(inputDataList, dataSelection, peakSmooth, peakUpperLimit);
                rsrpAverage.Number = averageValue.ToString("n2") + unit;
                /* MessageBox.Show("min:" + minimumValue + "\nmax:" + maximumValue + "\navg:" + averageValue); --> samo za test podataka */

                List<DateTime> listTime = new List<DateTime>();
                List<float> listRSRP = new List<float>();


                for (int i = 0; i < inputDataList.Count; i++)
                {
                    listTime.Add(inputDataList[i].Time);
                    listRSRP.Add((float)inputDataList[i].RSRQ);
                }

                InsertGraph(inputDataList, "RSRQ", peakSmooth, peakUpperLimit);
                tabSelector = "RSRQ";
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
                rsrpMax.Number = minimumValue.ToString() + unit;
                double maximumValue = CalculateMinimum(inputDataList, dataSelection);
                rsrpMin.Number = maximumValue.ToString() + unit;
                double averageValue = CalculateAverage(inputDataList, dataSelection, peakSmooth, peakUpperLimit);
                rsrpAverage.Number = averageValue.ToString("n2") + unit;
                /* MessageBox.Show("min:" + minimumValue + "\nmax:" + maximumValue + "\navg:" + averageValue); --> samo za test podataka */

                List<DateTime> listTime = new List<DateTime>();
                List<float> listRSRP = new List<float>();


                for (int i = 0; i < inputDataList.Count; i++)
                {
                    listTime.Add(inputDataList[i].Time);
                    listRSRP.Add((float)inputDataList[i].RSRQ);
                }

                InsertGraph(inputDataList, "SINR", peakSmooth, peakUpperLimit);
                tabSelector = "SINR";
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
                rsrpMax.Number = minimumValue.ToString() + "";
                double maximumValue = CalculateMinimum(inputDataList, dataSelection);
                rsrpMin.Number = maximumValue.ToString() + "";
                double averageValue = Math.Floor(CalculateAverage(inputDataList, dataSelection)); // CQI je cjelobrojna vrijednost
                rsrpAverage.Number = averageValue.ToString("n0") + "";
                /* MessageBox.Show("min:" + minimumValue + "\nmax:" + maximumValue + "\navg:" + averageValue); --> samo za test podataka */

                List<DateTime> listTime = new List<DateTime>();
                List<float> listRSRP = new List<float>();


                for (int i = 0; i < inputDataList.Count; i++)
                {
                    listTime.Add(inputDataList[i].Time);
                    listRSRP.Add((float)inputDataList[i].RSRQ);
                }

                InsertGraph(inputDataList, "CQI");
                tabSelector = "CQI";
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

                double minimumValue = CalculateMaximum(inputDataList, dataSelection);
                rsrpMax.Number = minimumValue.ToString() + unit;
                double maximumValue = CalculateMinimum(inputDataList, dataSelection);
                rsrpMin.Number = maximumValue.ToString() + unit;
                double averageValue = CalculateAverage(inputDataList, dataSelection);
                rsrpAverage.Number = averageValue.ToString("n2") + unit;
                /* MessageBox.Show("min:" + minimumValue + "\nmax:" + maximumValue + "\navg:" + averageValue); --> samo za test podataka */

                List<DateTime> listTime = new List<DateTime>();
                List<float> listRSRP = new List<float>();


                for (int i = 0; i < inputDataList.Count; i++)
                {
                    listTime.Add(inputDataList[i].Time);
                    listRSRP.Add((float)inputDataList[i].RSRQ);
                }

                InsertGraph(inputDataList, "Ping");
                tabSelector = "Ping";
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
                rsrpMax.Number = (minimumValue * 8).ToString("n2") + unit;
                double maximumValue = CalculateMinimum(inputDataList, dataSelection);
                rsrpMin.Number = (maximumValue * 8).ToString("n2") + unit;
                double averageValue = CalculateAverage(inputDataList, dataSelection);
                rsrpAverage.Number = (averageValue * 8).ToString("n2") + unit;
                /* MessageBox.Show("min:" + minimumValue + "\nmax:" + maximumValue + "\navg:" + averageValue); --> samo za test podataka */

                List<DateTime> listTime = new List<DateTime>();
                List<float> listRSRP = new List<float>();


                for (int i = 0; i < inputDataList.Count; i++)
                {
                    listTime.Add(inputDataList[i].Time);
                    listRSRP.Add((float)inputDataList[i].RSRQ);
                }

                InsertGraph(inputDataList, "Downlink");
                tabSelector = "Downlink";
            }
            else
            {
                var warningWindow = new UnloadedWarning();
                warningWindow.Show();
            }
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

        private PlotModel model; //model mora biti dostupan klasi zbog interakcije metoda grafa i exportera

        public void InsertGraph(List<BaseCsvData> inputList, string dataSelection)
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

            for (int i = 0; i < inputDataList.Count; i++) // ---> test za dualno pokazivanje grafa
            {
                if (inputDataList[i].Tech == "EN-DC")
                {
                    // uzimanje vrijednosti, dataSelection definira koji property 
                    object dataValue = inputDataList[i].GetType().GetProperty(dataSelection).GetValue(inputDataList[i]);

                    if (dataValue != null)
                    {
                        seriesRed.Points.Add(new DataPoint(DateTimeAxis.ToDouble(inputDataList[i].Time), Convert.ToDouble(dataValue))); // vrijednost je 5G, upisujem
                        seriesBlue.Points.Add(new DataPoint(DateTimeAxis.ToDouble(inputDataList[i].Time), Double.NaN)); // u 4G upisujem nullove
                    }
                    else
                    {
                        seriesRed.Points.Add(new DataPoint(DateTimeAxis.ToDouble(inputDataList[i].Time), Double.NaN)); // nema 5G vrijednosti                        
                    }
                }
                else
                {
                    object dataValue = inputDataList[i].GetType().GetProperty(dataSelection).GetValue(inputDataList[i]);

                    if (dataValue != null)
                    {
                        seriesBlue.Points.Add(new DataPoint(DateTimeAxis.ToDouble(inputDataList[i].Time), Convert.ToDouble(dataValue))); // vrijednost je 4G, upisujem
                        seriesRed.Points.Add(new DataPoint(DateTimeAxis.ToDouble(inputDataList[i].Time), Double.NaN)); // u 5G upisujem nullove
                    }
                    else
                    {
                        seriesBlue.Points.Add(new DataPoint(DateTimeAxis.ToDouble(inputDataList[i].Time), Double.NaN)); // nema 4G vrijednosti
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

        public void InsertGraph(List<BaseCsvData> inputList, string dataSelection, bool peakNormalization, int peakLimit)
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

            for (int i = 0; i < inputDataList.Count; i++)
            {
                // uzimanje vrijednosti, dataSelection definira koji property 
                object dataValue = inputDataList[i].GetType().GetProperty(dataSelection).GetValue(inputDataList[i]);

                if (inputDataList[i].Tech == "EN-DC")
                {
                    if (dataValue != null)
                    {
                        if (peakNormalization == true && (int)dataValue < peakLimit)
                        {
                            seriesRed.Points.Add(new DataPoint(DateTimeAxis.ToDouble(inputDataList[i].Time), Convert.ToDouble(dataValue))); // vrijednost je 5G i zadovoljava uvjete
                            seriesBlue.Points.Add(new DataPoint(DateTimeAxis.ToDouble(inputDataList[i].Time), Double.NaN)); // u 4G upisujem nullove
                        }
                        else
                        {
                            seriesRed.Points.Add(new DataPoint(DateTimeAxis.ToDouble(inputDataList[i].Time), Double.NaN)); // 5G vrijednost ne zadovoljava uvjete
                        }
                    }
                    else
                    {
                        seriesRed.Points.Add(new DataPoint(DateTimeAxis.ToDouble(inputDataList[i].Time), Double.NaN)); // nema 5G vrijednosti
                    }
                }
                else
                {
                    if (dataValue != null)
                    {
                        if (peakNormalization == true && (int)dataValue < peakLimit)
                        {
                            seriesBlue.Points.Add(new DataPoint(DateTimeAxis.ToDouble(inputDataList[i].Time), Convert.ToDouble(dataValue))); // vrijednost je 4G i zadovoljava uvjete
                            seriesRed.Points.Add(new DataPoint(DateTimeAxis.ToDouble(inputDataList[i].Time), Double.NaN)); // u 5G upisujem nullove
                        }
                        else
                        {
                            seriesBlue.Points.Add(new DataPoint(DateTimeAxis.ToDouble(inputDataList[i].Time), Double.NaN)); // 4G vrijednost ne zadovoljava uvjete
                        }
                    }
                    else
                    {
                        seriesBlue.Points.Add(new DataPoint(DateTimeAxis.ToDouble(inputDataList[i].Time), Double.NaN)); // nema 4G vrijednosti
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

        public void ExportGraph(object sender, RoutedEventArgs e)
        {
            if(loadComplete == true)
            {
                // dialog window
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "PNG Image (*.png)|*.png";
                saveFileDialog.Title = "Export Chart as PNG";
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
                }
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
                                if (value >= interval.lowerLimit && value < interval.upperLimit)
                                {
                                    color = interval.colorLimit;
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



    }
    
}
