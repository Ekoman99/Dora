using OxyPlot.Axes;
using OxyPlot.Series;
using OxyPlot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using OxyPlot.Wpf;

namespace Dora.Data
{
    public static class VisualisationEngine
    { 
        public static PlotView GraphView(PlotModel model)
        {
            var oxyplotChart = new PlotView
            {
                Model = model,
                Background = Brushes.Transparent
            };

            return oxyplotChart;
        }

        public static PlotView LineGraph(List<BaseCsvData> inputList, string dataSelection)
        {
            var model = LineModel(inputList, dataSelection);
            return GraphView(model);
        }

        public static PlotView StemGraph(List<BaseCsvData> inputList, string dataSelection)
        {
            var model = StemModel(inputList, dataSelection);
            return GraphView(model);
        }

        public static PlotView LineGraph(List<BaseCsvData> inputList, string dataSelection, bool peakNormalization, int peakLimit)
        {
            var model = LineModel(inputList, dataSelection, peakNormalization, peakLimit);
            return GraphView(model);
        }

        public static PlotView StemGraph(List<BaseCsvData> inputList, string dataSelection, bool peakNormalization, int peakLimit)
        {
            var model = StemModel(inputList, dataSelection, peakNormalization, peakLimit);
            return GraphView(model);
        }

        public static PlotModel LineModel(List<BaseCsvData> inputList, string dataSelection)
        {
            // kreiranje modela za plotanje
            var model = new PlotModel
            {
                Background = OxyColors.Transparent,
                PlotAreaBorderColor = OxyColors.Transparent,
            };

            // serija točaka
            var seriesBlue = new LineSeries // LTE
            {
                Color = OxyColor.Parse("#349DC8"),
            };

            var seriesRed = new LineSeries // NR
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

            return model;
        }

        private static PlotModel LineModel(List<BaseCsvData> inputList, string dataSelection, bool peakNormalization, int peakLimit)
        {
            // kreiranje modela za plotanje
            var model = new PlotModel
            {
                Background = OxyColor.Parse("#00000000"),
                PlotAreaBorderColor = OxyColor.Parse("#00000000"),
            };

            // serija točaka
            var seriesBlue = new LineSeries // LTE
            {
                Color = OxyColor.Parse("#349DC8"),
            };

            var seriesRed = new LineSeries // NR
            {
                Color = OxyColor.Parse("#C41F1F"),
            };

            for (int i = 0; i < inputList.Count; i++)
            {
                // uzimanje vrijednosti, dataSelection definira koji property 
                object dataValue = inputList[i].GetType().GetProperty(dataSelection).GetValue(inputList[i]);

                if (inputList[i].Tech == "EN-DC")
                {
                    if (dataValue != null)
                    {
                        if (peakNormalization == true && (int)dataValue < peakLimit)
                        {
                            seriesRed.Points.Add(new DataPoint(DateTimeAxis.ToDouble(inputList[i].Time), Convert.ToDouble(dataValue))); // vrijednost je 5G i zadovoljava uvjete
                            seriesBlue.Points.Add(new DataPoint(DateTimeAxis.ToDouble(inputList[i].Time), double.NaN)); // u 4G upisujem nullove
                        }
                        else
                        {
                            seriesRed.Points.Add(new DataPoint(DateTimeAxis.ToDouble(inputList[i].Time), double.NaN)); // 5G vrijednost ne zadovoljava uvjete
                        }
                    }
                    else
                    {
                        seriesRed.Points.Add(new DataPoint(DateTimeAxis.ToDouble(inputList[i].Time), double.NaN)); // nema 5G vrijednosti
                    }
                }
                else
                {
                    if (dataValue != null)
                    {
                        if (peakNormalization == true && (int)dataValue < peakLimit)
                        {
                            seriesBlue.Points.Add(new DataPoint(DateTimeAxis.ToDouble(inputList[i].Time), Convert.ToDouble(dataValue))); // vrijednost je 4G i zadovoljava uvjete
                            seriesRed.Points.Add(new DataPoint(DateTimeAxis.ToDouble(inputList[i].Time), double.NaN)); // u 5G upisujem nullove
                        }
                        else
                        {
                            seriesBlue.Points.Add(new DataPoint(DateTimeAxis.ToDouble(inputList[i].Time), double.NaN)); // 4G vrijednost ne zadovoljava uvjete
                        }
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

            return model;
        }

        public static PlotModel StemModel(List<BaseCsvData> inputList, string dataSelection) // izgleda kao clustered column, minimalna prilagodba potrebna
        {
            // kreiranje modela za plotanje
            var model = new PlotModel
            {
                Background = OxyColors.Transparent,
                PlotAreaBorderColor = OxyColors.Transparent,
            };

            // serija točaka
            var seriesBlue = new StemSeries // LTE
            {
                Color = OxyColor.Parse("#349DC8"),
            };

            var seriesRed = new StemSeries // NR
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

            return model;
        }

        private static PlotModel StemModel(List<BaseCsvData> inputList, string dataSelection, bool peakNormalization, int peakLimit)
        {
            // kreiranje modela za plotanje
            var model = new PlotModel
            {
                Background = OxyColors.Transparent,
                PlotAreaBorderColor = OxyColors.Transparent,
            };

            // serija točaka
            var seriesBlue = new StemSeries // LTE
            {
                Color = OxyColor.Parse("#349DC8"),
            };

            var seriesRed = new StemSeries // NR
            {
                Color = OxyColor.Parse("#C41F1F"),
            };

            for (int i = 0; i < inputList.Count; i++)
            {
                // uzimanje vrijednosti, dataSelection definira koji property 
                object dataValue = inputList[i].GetType().GetProperty(dataSelection).GetValue(inputList[i]);

                if (inputList[i].Tech == "EN-DC")
                {
                    if (dataValue != null)
                    {
                        if (peakNormalization == true && (int)dataValue < peakLimit)
                        {
                            seriesRed.Points.Add(new DataPoint(DateTimeAxis.ToDouble(inputList[i].Time), Convert.ToDouble(dataValue))); // vrijednost je 5G i zadovoljava uvjete
                            seriesBlue.Points.Add(new DataPoint(DateTimeAxis.ToDouble(inputList[i].Time), double.NaN)); // u 4G upisujem nullove
                        }
                        else
                        {
                            seriesRed.Points.Add(new DataPoint(DateTimeAxis.ToDouble(inputList[i].Time), double.NaN)); // 5G vrijednost ne zadovoljava uvjete
                        }
                    }
                    else
                    {
                        seriesRed.Points.Add(new DataPoint(DateTimeAxis.ToDouble(inputList[i].Time), double.NaN)); // nema 5G vrijednosti
                    }
                }
                else
                {
                    if (dataValue != null)
                    {
                        if (peakNormalization == true && (int)dataValue < peakLimit)
                        {
                            seriesBlue.Points.Add(new DataPoint(DateTimeAxis.ToDouble(inputList[i].Time), Convert.ToDouble(dataValue))); // vrijednost je 4G i zadovoljava uvjete
                            seriesRed.Points.Add(new DataPoint(DateTimeAxis.ToDouble(inputList[i].Time), double.NaN)); // u 5G upisujem nullove
                        }
                        else
                        {
                            seriesBlue.Points.Add(new DataPoint(DateTimeAxis.ToDouble(inputList[i].Time), double.NaN)); // 4G vrijednost ne zadovoljava uvjete
                        }
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

            return model;
        }

        private static PlotModel AreaModel(List<BaseCsvData> inputList, string dataSelection)
        {
            // kreiranje modela za plotanje
            var model = new PlotModel
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

            for (int i = 0; i < inputList.Count - 1; i++)
            {
                // Calculate time difference between consecutive points
                double timeDiff = (inputList[i + 1].Time - inputList[i].Time).TotalSeconds;

                if (timeDiff > 1.5)
                {
                    // Calculate the number of steps to add based on the time difference
                    int steps = (int)Math.Floor(timeDiff / 1.5);

                    // Add zeros and intermediate timestamps to both series
                    for (int j = 0; j < steps; j++)
                    {
                        DateTime intermediateTime = inputList[i].Time.AddSeconds((j + 1) * 1.5);

                        seriesBlue.Points.Add(new DataPoint(DateTimeAxis.ToDouble(intermediateTime), 0));
                        seriesRed.Points.Add(new DataPoint(DateTimeAxis.ToDouble(intermediateTime), 0));
                    }
                }

                // Add the current data point
                if (inputList[i].Tech == "EN-DC")
                {
                    object dataValue = inputList[i].GetType().GetProperty(dataSelection).GetValue(inputList[i]);

                    if (dataValue != null)
                    {
                        seriesRed.Points.Add(new DataPoint(DateTimeAxis.ToDouble(inputList[i].Time), Convert.ToDouble(dataValue)));
                        seriesBlue.Points.Add(new DataPoint(DateTimeAxis.ToDouble(inputList[i].Time), 0));
                    }
                    else
                    {
                        seriesRed.Points.Add(new DataPoint(DateTimeAxis.ToDouble(inputList[i].Time), 0));
                        seriesBlue.Points.Add(new DataPoint(DateTimeAxis.ToDouble(inputList[i].Time), 0));
                    }
                }
                else
                {
                    object dataValue = inputList[i].GetType().GetProperty(dataSelection).GetValue(inputList[i]);

                    if (dataValue != null)
                    {
                        seriesBlue.Points.Add(new DataPoint(DateTimeAxis.ToDouble(inputList[i].Time), Convert.ToDouble(dataValue)));
                        seriesRed.Points.Add(new DataPoint(DateTimeAxis.ToDouble(inputList[i].Time), 0));
                    }
                    else
                    {
                        seriesBlue.Points.Add(new DataPoint(DateTimeAxis.ToDouble(inputList[i].Time), 0));
                        seriesRed.Points.Add(new DataPoint(DateTimeAxis.ToDouble(inputList[i].Time), 0));
                    }
                }
            }

            // Add the last data point
            int lastIndex = inputList.Count - 1;
            if (inputList[lastIndex].Tech == "EN-DC")
            {
                object dataValue = inputList[lastIndex].GetType().GetProperty(dataSelection).GetValue(inputList[lastIndex]);

                if (dataValue != null)
                {
                    seriesRed.Points.Add(new DataPoint(DateTimeAxis.ToDouble(inputList[lastIndex].Time), Convert.ToDouble(dataValue)));
                    seriesBlue.Points.Add(new DataPoint(DateTimeAxis.ToDouble(inputList[lastIndex].Time), 0));
                }
                else
                {
                    seriesRed.Points.Add(new DataPoint(DateTimeAxis.ToDouble(inputList[lastIndex].Time), 0));
                    seriesBlue.Points.Add(new DataPoint(DateTimeAxis.ToDouble(inputList[lastIndex].Time), 0));
                }
            }
            else
            {
                object dataValue = inputList[lastIndex].GetType().GetProperty(dataSelection).GetValue(inputList[lastIndex]);

                if (dataValue != null)
                {
                    seriesBlue.Points.Add(new DataPoint(DateTimeAxis.ToDouble(inputList[lastIndex].Time), Convert.ToDouble(dataValue)));
                    seriesRed.Points.Add(new DataPoint(DateTimeAxis.ToDouble(inputList[lastIndex].Time), 0));
                }
                else
                {
                    seriesBlue.Points.Add(new DataPoint(DateTimeAxis.ToDouble(inputList[lastIndex].Time), 0));
                    seriesRed.Points.Add(new DataPoint(DateTimeAxis.ToDouble(inputList[lastIndex].Time), 0));
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

            return model;
        } // potrebna prilagodba, area tip nije najbolji zbog tipa podataka        

    }
}
