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
        // pretvorba PlotModela u oxyplotchart

        public static PlotView GraphView(PlotModel model)
        {
            var oxyplotChart = new PlotView
            {
                Model = model,
                Background = Brushes.Transparent
            };

            return oxyplotChart;
        }

        // funkcije za poziv modela

        public static PlotView LineGraph(List<BaseCsvData> inputList, string dataSelection, GraphConfig graphConfig)
        {
            var model = LineModel(inputList, dataSelection, graphConfig);
            return GraphView(model);
        }

        public static PlotView StemGraph(List<BaseCsvData> inputList, string dataSelection, GraphConfig graphConfig, int interpolationValue, bool interpolationState)
        {
            var model = StemModel(inputList, dataSelection, graphConfig, interpolationValue, interpolationState);
            return GraphView(model);
        }

        public static PlotView ColumnGraph(List<BaseCsvData> inputList, string dataSelection, bool peakNormalization, int peakLimit)
        {
            var model = ColumnModel(inputList, dataSelection, peakNormalization, peakLimit);
            return GraphView(model);
        }

        // funkcije koje generiraju modele

        private static PlotModel LineModel(List<BaseCsvData> inputList, string dataSelection, GraphConfig graphConfig)
        {
            var model = new PlotModel
            {
                Background = OxyColor.Parse(graphConfig.GraphBackground),
                PlotAreaBorderColor = OxyColors.Transparent,
            };

            var seriesBlue = new LineSeries // LTE
            {
                Color = OxyColor.Parse(graphConfig.LTEcolor),
            };

            var seriesRed = new LineSeries // NR
            {
                Color = OxyColor.Parse(graphConfig.NRcolor),
            };

            for (int i = 0; i < inputList.Count; i++)
            {
                object dataValue = inputList[i].GetType().GetProperty(dataSelection).GetValue(inputList[i]);

                if (inputList[i].Tech == "EN-DC")
                {
                    if (dataValue != null)
                    {
                        if (graphConfig.PeakNormalization == true && Convert.ToInt32(dataValue) < graphConfig.PeakLimit)
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
                        if (graphConfig.PeakNormalization == true && Convert.ToInt32(dataValue) < graphConfig.PeakLimit)
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

            var parsedColor = OxyColor.Parse(graphConfig.GraphElements);
            var gridlineColor = OxyColor.FromAColor(50, parsedColor);

            var xAxis = new DateTimeAxis
            {
                Position = AxisPosition.Bottom,
                Title = "Time",
                MajorGridlineColor = gridlineColor,
                MajorGridlineStyle = LineStyle.Solid,
                AxislineColor = parsedColor,
                TitleColor = parsedColor,
                TextColor = parsedColor,
                MinorTicklineColor = parsedColor,
                TicklineColor = parsedColor,
            };

            var yAxis = new LinearAxis
            {
                Position = AxisPosition.Left,
                Title = dataSelection,
                MajorGridlineColor = gridlineColor,
                MajorGridlineStyle = LineStyle.Solid,
                AxislineColor = parsedColor,
                TitleColor = parsedColor,
                TextColor = parsedColor,
                MinorTicklineColor = parsedColor,
                TicklineColor = parsedColor,
            };

            model.Axes.Add(xAxis);
            model.Axes.Add(yAxis);

            return model;
        }

        private static PlotModel StemModel(List<BaseCsvData> inputList, string dataSelection, GraphConfig graphConfig, int duplicationFactor = 1, bool interpolationState = false)
        {
            var model = new PlotModel
            {
                Background = OxyColor.Parse(graphConfig.GraphBackground),
                PlotAreaBorderColor = OxyColors.Transparent,
            };

            if (!interpolationState)
            {
                duplicationFactor = 1;
            }

            var seriesLTE = new StemSeries { Color = OxyColor.Parse(graphConfig.LTEcolor) }; // LTE
            var seriesNR = new StemSeries { Color = OxyColor.Parse(graphConfig.NRcolor) };  // NR

            var points = new List<(DateTime time, double value, string tech)>();

            for (int i = 0; i < inputList.Count; i++)
            {
                object dataValue = inputList[i].GetType().GetProperty(dataSelection).GetValue(inputList[i]);
                var time = inputList[i].Time;
                var tech = inputList[i].Tech;

                if (dataValue != null && graphConfig.PeakNormalization && Convert.ToInt32(dataValue) < graphConfig.PeakLimit)
                {
                    points.Add((time, Convert.ToDouble(dataValue), tech));
                }
            }

            // sortiranje po vremenu (eliminacija potencijalnih grešaka)
            //points = points.OrderBy(p => p.time).ToList();

            // dodavanje točaka u kontinuiranim segmentima
            List<(DateTime time, double value, string tech)> DuplicatePoints(List<(DateTime time, double value, string tech)> originalPoints)
            {
                if (originalPoints.Count < 2 || duplicationFactor <= 1) return originalPoints; // ako je u listi jedna točka ili faktor dupliciranja iznosi 1 ili manje vraćaju se originalne točke

                var result = new List<(DateTime time, double value, string tech)>();

                for (int i = 0; i < originalPoints.Count - 1; i++)
                {
                    var current = originalPoints[i];
                    var next = originalPoints[i + 1];

                    if (current.tech == next.tech)
                    {
                        for (int j = 0; j < duplicationFactor; j++)
                        {
                            var fraction = (double)j / duplicationFactor;
                            var interpolatedTime = current.time.AddTicks((long)(fraction * (next.time - current.time).Ticks));
                            result.Add((interpolatedTime, current.value, current.tech));
                        }
                    }
                    else
                    {
                        result.Add(current);
                    }
                }

                if (originalPoints.Count > 0)
                {
                    result.Add(originalPoints[originalPoints.Count - 1]);
                }

                return result;
            }

            var interpolatedPoints = DuplicatePoints(points);

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

            var allUniqueTimes = new HashSet<DateTime>(); // hash tablica svih vremenskih točaka
            bluePoints.ForEach(p => allUniqueTimes.Add(p.time)); // dodavanje svih vremena u hashset
            redPoints.ForEach(p => allUniqueTimes.Add(p.time));
            var sortedTimes = allUniqueTimes.OrderBy(t => t).ToList(); // sortirane sve jedinstvene vremenske točke u list

            
            foreach (var time in sortedTimes)
            {
                var bluePoint = bluePoints.FirstOrDefault(p => p.time == time);
                var redPoint = redPoints.FirstOrDefault(p => p.time == time);

                seriesLTE.Points.Add(new DataPoint(DateTimeAxis.ToDouble(time),
                    bluePoint.time != default ? bluePoint.value : double.NaN));

                seriesNR.Points.Add(new DataPoint(DateTimeAxis.ToDouble(time),
                    redPoint.time != default ? redPoint.value : double.NaN));
            }

            model.Series.Add(seriesLTE);
            model.Series.Add(seriesNR);

            var parsedColor = OxyColor.Parse(graphConfig.GraphElements);
            var gridlineColor = OxyColor.FromAColor(50, parsedColor);

            // definiranje osi
            var xAxis = new DateTimeAxis
            {
                Position = AxisPosition.Bottom,
                Title = "Time",
                MajorGridlineColor = gridlineColor,
                MajorGridlineStyle = LineStyle.Solid,
                AxislineColor = parsedColor,
                TitleColor = parsedColor,
                TextColor = parsedColor,
                MinorTicklineColor = parsedColor,
                TicklineColor = parsedColor,
            };

            var yAxis = new LinearAxis
            {
                Position = AxisPosition.Left,
                Title = dataSelection,
                MajorGridlineColor = gridlineColor,
                MajorGridlineStyle = LineStyle.Solid,
                AxislineColor = parsedColor,
                TitleColor = parsedColor,
                TextColor = parsedColor,
                MinorTicklineColor = parsedColor,
                TicklineColor = parsedColor,
            };

            model.Axes.Add(xAxis);
            model.Axes.Add(yAxis);

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


            var startTime = inputList[0].Time;
            var endTime = inputList[inputList.Count - 1].Time;
            var totalSeconds = (endTime - startTime).TotalSeconds;
            var intervalSeconds = totalSeconds / 20;

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

            for (int i = 0; i < inputList.Count; i++)
            {
                var currentTime = inputList[i].Time;
                var elapsedSeconds = (currentTime - startTime).TotalSeconds;

                if (i == 0 || i == inputList.Count - 1 ||
                    i % Math.Max(1, inputList.Count / 20) == 0)
                {
                    categoryAxis.Labels.Add(currentTime.ToString("HH:mm:ss"));
                }
                else
                {
                    categoryAxis.Labels.Add("");
                }

                seriesBlue.Items.Add(new BarItem { Value = 0 });
                seriesRed.Items.Add(new BarItem { Value = 0 });
            }

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

    }
}
