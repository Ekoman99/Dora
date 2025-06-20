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
using Dora.Models;

namespace Dora.Data
{
    public static class VisualisationEngine
    { 
        //oxyplotchart

        public static PlotView GraphView(PlotModel model)
        {
            var oxyplotChart = new PlotView
            {
                Model = model,
                Background = Brushes.Transparent
            };

            return oxyplotChart;
        }

        //graphs

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

        public static PlotView StemGraph(List<BaseCsvData> inputList, string dataSelection, GraphConfig graphConfig, int interpolationValue, bool interpolationState)
        {
            var model = StemModel(inputList, dataSelection, graphConfig, interpolationValue, interpolationState);
            return GraphView(model);
        }

        public static PlotView LineGraph(List<BaseCsvData> inputList, string dataSelection, bool peakNormalization, int peakLimit, int interpolationValue, bool interpolationState)
        {
            var model = LineModel(inputList, dataSelection, peakLimit, interpolationValue, peakNormalization, interpolationState);
            return GraphView(model);
        }

        public static PlotView ColumnGraph(List<BaseCsvData> inputList, string dataSelection)
        {
            var model = ColumnModel(inputList, dataSelection);
            return GraphView(model);
        }

        public static PlotView ColumnGraph(List<BaseCsvData> inputList, string dataSelection, bool peakNormalization, int peakLimit)
        {
            var model = ColumnModel(inputList, dataSelection, peakNormalization, peakLimit);
            return GraphView(model);
        }


        //model selection

        /* public static PlotModel SelectModel(string tabSelect, bool graphType, List<BaseCsvData> inputList, )
         {
             var exportModel = new PlotModel();

             switch (tabSelect)
             {
                 case "RSRP":
                     {
                         if (graphType)
                         {
                             exportModel = LineGraph()
                         }
                     }
             }


         }*/

        //models

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

        private static PlotModel StemModel(List<BaseCsvData> inputList, string dataSelection, GraphConfig graphConfig, int interpolationFactor = 1, bool interpolationState = false)
        {
            var model = new PlotModel
            {
                Background = OxyColors.Transparent,
                PlotAreaBorderColor = OxyColors.Transparent,
            };

            if (!interpolationState)
            {
                interpolationFactor = 1;
            }

            var seriesBlue = new StemSeries { Color = OxyColor.Parse("#349DC8") }; // LTE
            var seriesRed = new StemSeries { Color = OxyColor.Parse("#C41F1F") };  // NR

            // Create a list of all points with their technology type
            var allPointsWithTech = new List<(DateTime time, double value, string tech)>();

            for (int i = 0; i < inputList.Count; i++)
            {
                object dataValue = inputList[i].GetType().GetProperty(dataSelection).GetValue(inputList[i]);
                var time = inputList[i].Time;
                var tech = inputList[i].Tech;

                if (dataValue != null && graphConfig.PeakNormalization && Convert.ToInt32(dataValue) < graphConfig.PeakLimit)
                {
                    allPointsWithTech.Add((time, Convert.ToDouble(dataValue), tech));
                }
            }

            // Sort by time to ensure proper order
            allPointsWithTech = allPointsWithTech.OrderBy(p => p.time).ToList();

            // Function to interpolate points only within continuous technology segments
            List<(DateTime time, double value, string tech)> InterpolatePointsWithTechBoundaries(List<(DateTime time, double value, string tech)> points)
            {
                if (points.Count < 2 || interpolationFactor <= 1) return points;

                var result = new List<(DateTime time, double value, string tech)>();

                for (int i = 0; i < points.Count - 1; i++)
                {
                    var current = points[i];
                    var next = points[i + 1];

                    // Add interpolated points only if the technology doesn't change
                    if (current.tech == next.tech)
                    {
                        for (int j = 0; j < interpolationFactor; j++)
                        {
                            var fraction = (double)j / interpolationFactor;
                            var interpolatedTime = current.time.AddTicks((long)(fraction * (next.time - current.time).Ticks));
                            result.Add((interpolatedTime, current.value, current.tech));
                        }
                    }
                    else
                    {
                        // Technology changes, just add the current point without interpolation
                        result.Add(current);
                    }
                }

                // Add the last point
                if (points.Count > 0)
                {
                    result.Add(points[points.Count - 1]);
                }

                return result;
            }

            // Interpolate all points while respecting technology boundaries
            var interpolatedPoints = InterpolatePointsWithTechBoundaries(allPointsWithTech);

            // Separate interpolated points by technology
            var bluePoints = new List<(DateTime time, double value)>();
            var redPoints = new List<(DateTime time, double value)>();

            foreach (var point in interpolatedPoints)
            {
                if (point.tech == "EN-DC")
                {
                    redPoints.Add((point.time, point.value));
                }
                else
                {
                    bluePoints.Add((point.time, point.value));
                }
            }

            // Get all unique times
            var allTimes = new HashSet<DateTime>();
            bluePoints.ForEach(p => allTimes.Add(p.time));
            redPoints.ForEach(p => allTimes.Add(p.time));
            var sortedTimes = allTimes.OrderBy(t => t).ToList();

            // Add points to series, using NaN for missing values
            foreach (var time in sortedTimes)
            {
                var bluePoint = bluePoints.FirstOrDefault(p => p.time == time);
                var redPoint = redPoints.FirstOrDefault(p => p.time == time);

                seriesBlue.Points.Add(new DataPoint(DateTimeAxis.ToDouble(time),
                    bluePoint.time != default ? bluePoint.value : double.NaN));

                seriesRed.Points.Add(new DataPoint(DateTimeAxis.ToDouble(time),
                    redPoint.time != default ? redPoint.value : double.NaN));
            }

            model.Series.Add(seriesBlue);
            model.Series.Add(seriesRed);

            // Axes configuration
            var xAxis = new DateTimeAxis
            {
                Position = AxisPosition.Bottom,
                Title = "Time",
                MajorGridlineColor = OxyColor.FromAColor(50, OxyColors.White),
                MajorGridlineStyle = LineStyle.Solid,
                AxislineColor = OxyColor.FromRgb(255, 255, 255),
                TitleColor = OxyColor.FromRgb(255, 255, 255),
                TextColor = OxyColor.FromRgb(255, 255, 255),
                MinorTicklineColor = OxyColor.FromRgb(255, 255, 255),
                TicklineColor = OxyColor.FromRgb(255, 255, 255),
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

        private static PlotModel LineModel(List<BaseCsvData> inputList, string dataSelection, int peakLimit, int interpolationFactor = 1, bool peakNormalization = false, bool interpolationState = false)
        {
            var model = new PlotModel
            {
                Background = OxyColor.Parse("#00000000"),
                PlotAreaBorderColor = OxyColor.Parse("#00000000"),
            };

            if (!interpolationState)
            {
                interpolationFactor = 1;
            }

            var seriesBlue = new LineSeries { Color = OxyColor.Parse("#349DC8") }; // LTE
            var seriesRed = new LineSeries { Color = OxyColor.Parse("#C41F1F") };  // NR

            // List of all points
            var bluePoints = new List<(DateTime time, double value)>();
            var redPoints = new List<(DateTime time, double value)>();

            for (int i = 0; i < inputList.Count; i++)
            {
                object dataValue = inputList[i].GetType().GetProperty(dataSelection).GetValue(inputList[i]);
                var time = inputList[i].Time;

                if (inputList[i].Tech == "EN-DC")
                {
                    if (dataValue != null && peakNormalization && Convert.ToInt32(dataValue) < peakLimit)
                    {
                        redPoints.Add((time, Convert.ToDouble(dataValue)));
                    }
                }
                else
                {
                    if (dataValue != null && peakNormalization && Convert.ToInt32(dataValue) < peakLimit)
                    {
                        bluePoints.Add((time, Convert.ToDouble(dataValue)));
                    }
                }
            }

            // Interpolation function
            List<(DateTime time, double value)> InterpolatePoints(List<(DateTime time, double value)> points)
            {
                if (points.Count < 2 || interpolationFactor <= 1) return points;

                var interpolatedPoints = new List<(DateTime time, double value)>();

                for (int i = 0; i < points.Count - 1; i++)
                {
                    var start = points[i];
                    var end = points[i + 1];

                    for (int j = 0; j < interpolationFactor; j++)
                    {
                        var fraction = (double)j / interpolationFactor;
                        var interpolatedTime = start.time.AddTicks((long)(fraction * (end.time - start.time).Ticks));

                        interpolatedPoints.Add((interpolatedTime, start.value)); // repeat the value
                    }
                }

                // Add the last point
                if (points.Count > 0)
                {
                    interpolatedPoints.Add(points[points.Count - 1]);
                }

                return interpolatedPoints;
            }

            // Call interpolation function
            var interpolatedBluePoints = InterpolatePoints(bluePoints);
            var interpolatedRedPoints = InterpolatePoints(redPoints);

            // Sort times
            var allTimes = new HashSet<DateTime>();
            interpolatedBluePoints.ForEach(p => allTimes.Add(p.time));
            interpolatedRedPoints.ForEach(p => allTimes.Add(p.time));
            var sortedTimes = allTimes.OrderBy(t => t).ToList();

            // Add points, handling missing values
            foreach (var time in sortedTimes)
            {
                var blueValue = interpolatedBluePoints.FirstOrDefault(p => p.time == time).value;
                var redValue = interpolatedRedPoints.FirstOrDefault(p => p.time == time).value;

                seriesBlue.Points.Add(new DataPoint(DateTimeAxis.ToDouble(time),
                    interpolatedBluePoints.Any(p => p.time == time) ? blueValue : double.NaN));

                seriesRed.Points.Add(new DataPoint(DateTimeAxis.ToDouble(time),
                    interpolatedRedPoints.Any(p => p.time == time) ? redValue : double.NaN));
            }

            model.Series.Add(seriesBlue);
            model.Series.Add(seriesRed);

            // Axes
            var xAxis = new DateTimeAxis
            {
                Position = AxisPosition.Bottom,
                Title = "Time",
                MajorGridlineColor = OxyColor.FromAColor(50, OxyColors.White),
                MajorGridlineStyle = LineStyle.Solid,
                AxislineColor = OxyColor.FromRgb(255, 255, 255),
                TitleColor = OxyColor.FromRgb(255, 255, 255),
                TextColor = OxyColor.FromRgb(255, 255, 255),
                MinorTicklineColor = OxyColor.FromRgb(255, 255, 255),
                TicklineColor = OxyColor.FromRgb(255, 255, 255),
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

        private static PlotModel ColumnModel(List<BaseCsvData> inputList, string dataSelection)
        {
            var model = new PlotModel
            {
                Background = OxyColors.Transparent,
                PlotAreaBorderColor = OxyColors.Transparent,
            };

            // Create series for both technologies
            var seriesBlue = new BarSeries // LTE
            {
                Title = "LTE",
                FillColor = OxyColor.Parse("#349DC8"),
                StrokeColor = OxyColor.Parse("#349DC8"),
                StrokeThickness = 1,
                XAxisKey = "Value",
                YAxisKey = "Category",
                BarWidth = 0.8
            };

            var seriesRed = new BarSeries // NR
            {
                Title = "NR",
                FillColor = OxyColor.Parse("#C41F1F"),
                StrokeColor = OxyColor.Parse("#C41F1F"),
                StrokeThickness = 1,
                XAxisKey = "Value",
                YAxisKey = "Category",
                BarWidth = 0.8
            };

            // Create category axis with time labels
            var categoryAxis = new CategoryAxis
            {
                Position = AxisPosition.Bottom,
                Key = "Category",
                Title = "Time",
                MajorGridlineColor = OxyColor.FromAColor(50, OxyColors.White),
                MajorGridlineStyle = LineStyle.Solid,
                AxislineColor = OxyColor.FromRgb(255, 255, 255),
                TitleColor = OxyColor.FromRgb(255, 255, 255),
                TextColor = OxyColor.FromRgb(255, 255, 255),
                MinorTicklineColor = OxyColor.FromRgb(255, 255, 255),
                TicklineColor = OxyColor.FromRgb(255, 255, 255)
            };

            // Add data points and time labels
            for (int i = 0; i < inputList.Count; i++)
            {
                categoryAxis.Labels.Add(inputList[i].Time.ToString("HH:mm:ss"));

                if (inputList[i].Tech == "EN-DC")
                {
                    object dataValue = inputList[i].GetType().GetProperty(dataSelection).GetValue(inputList[i]);

                    if (dataValue != null)
                    {
                        seriesRed.Items.Add(new BarItem { Value = Convert.ToDouble(dataValue) });
                        seriesBlue.Items.Add(new BarItem { Value = double.NaN });
                    }
                    else
                    {
                        seriesRed.Items.Add(new BarItem { Value = double.NaN });
                    }
                }
                else
                {
                    object dataValue = inputList[i].GetType().GetProperty(dataSelection).GetValue(inputList[i]);

                    if (dataValue != null)
                    {
                        seriesBlue.Items.Add(new BarItem { Value = Convert.ToDouble(dataValue) });
                        seriesRed.Items.Add(new BarItem { Value = double.NaN });
                    }
                    else
                    {
                        seriesBlue.Items.Add(new BarItem { Value = double.NaN });
                    }
                }
            }

            // Add value axis
            var valueAxis = new LinearAxis
            {
                Position = AxisPosition.Left,
                Key = "Value",
                Title = dataSelection,
                MajorGridlineColor = OxyColor.FromAColor(50, OxyColors.White),
                MajorGridlineStyle = LineStyle.Solid,
                AxislineColor = OxyColor.FromRgb(255, 255, 255),
                TitleColor = OxyColor.FromRgb(255, 255, 255),
                TextColor = OxyColor.FromRgb(255, 255, 255),
                MinorTicklineColor = OxyColor.FromRgb(255, 255, 255),
                TicklineColor = OxyColor.FromRgb(255, 255, 255)
            };

            model.Axes.Add(categoryAxis);
            model.Axes.Add(valueAxis);
            model.Series.Add(seriesBlue);
            model.Series.Add(seriesRed);

            return model;
        }

        private static PlotModel ColumnModel(List<BaseCsvData> inputList, string dataSelection, bool peakNormalization, int peakLimit)
        {
            var model = new PlotModel
            {
                Background = OxyColors.Transparent,
                PlotAreaBorderColor = OxyColors.Transparent,
            };
            var seriesBlue = new BarSeries // LTE
            {
                Title = "LTE",
                FillColor = OxyColor.Parse("#349DC8"),
                StrokeColor = OxyColor.Parse("#349DC8"),
                StrokeThickness = 1,
                XAxisKey = "Value",
                YAxisKey = "Category",
                BarWidth = 0.8
            };
            var seriesRed = new BarSeries // NR
            {
                Title = "NR",
                FillColor = OxyColor.Parse("#C41F1F"),
                StrokeColor = OxyColor.Parse("#C41F1F"),
                StrokeThickness = 1,
                XAxisKey = "Value",
                YAxisKey = "Category",
                BarWidth = 0.8
            };

            // Calculate total time span and intervals
            var startTime = inputList[0].Time;
            var endTime = inputList[inputList.Count - 1].Time;  // Changed from [^1] to [Count - 1]
            var totalSeconds = (endTime - startTime).TotalSeconds;
            var intervalSeconds = totalSeconds / 20; // Split into 20 intervals

            var categoryAxis = new CategoryAxis
            {
                Position = AxisPosition.Bottom,
                Key = "Category",
                Title = "Time",
                MajorGridlineColor = OxyColor.FromAColor(50, OxyColors.White),
                MajorGridlineStyle = LineStyle.Solid,
                AxislineColor = OxyColor.FromRgb(255, 255, 255),
                TitleColor = OxyColor.FromRgb(255, 255, 255),
                TextColor = OxyColor.FromRgb(255, 255, 255),
                MinorTicklineColor = OxyColor.FromRgb(255, 255, 255),
                TicklineColor = OxyColor.FromRgb(255, 255, 255)
            };

            // Initialize both series with zero values for all time points
            for (int i = 0; i < inputList.Count; i++)
            {
                // Only add labels for strategic points
                var currentTime = inputList[i].Time;
                var elapsedSeconds = (currentTime - startTime).TotalSeconds;

                if (i == 0 || i == inputList.Count - 1 || // Always show first and last
                    i % Math.Max(1, inputList.Count / 20) == 0) // Show approximately 20 points
                {
                    categoryAxis.Labels.Add(currentTime.ToString("HH:mm:ss"));
                }
                else
                {
                    categoryAxis.Labels.Add(""); // Empty label for other points
                }

                seriesBlue.Items.Add(new BarItem { Value = 0 });
                seriesRed.Items.Add(new BarItem { Value = 0 });
            }

            // Fill in actual values where they exist
            for (int i = 0; i < inputList.Count; i++)
            {
                object dataValue = inputList[i].GetType().GetProperty(dataSelection).GetValue(inputList[i]);
                if (dataValue != null)
                {
                    double value = Convert.ToDouble(dataValue);
                    if (!peakNormalization || value < peakLimit)
                    {
                        if (inputList[i].Tech == "EN-DC")
                        {
                            seriesRed.Items[i] = new BarItem { Value = value };
                        }
                        else
                        {
                            seriesBlue.Items[i] = new BarItem { Value = value };
                        }
                    }
                }
            }

            var valueAxis = new LinearAxis
            {
                Position = AxisPosition.Left,
                Key = "Value",
                Title = dataSelection,
                MajorGridlineColor = OxyColor.FromAColor(50, OxyColors.White),
                MajorGridlineStyle = LineStyle.Solid,
                AxislineColor = OxyColor.FromRgb(255, 255, 255),
                TitleColor = OxyColor.FromRgb(255, 255, 255),
                TextColor = OxyColor.FromRgb(255, 255, 255),
                MinorTicklineColor = OxyColor.FromRgb(255, 255, 255),
                TicklineColor = OxyColor.FromRgb(255, 255, 255)
            };

            model.Axes.Add(categoryAxis);
            model.Axes.Add(valueAxis);
            model.Series.Add(seriesBlue);
            model.Series.Add(seriesRed);

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
