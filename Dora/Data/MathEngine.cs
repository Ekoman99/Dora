using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dora.Data
{
    static class MathEngine
    {
        public static double CalculateAverage(List<BaseCsvData> list, string dataSelection, bool peakNormalization = false, int peakLimit = int.MaxValue)
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
                            var value = Convert.ToDouble(propertyValue);
                            if (!peakNormalization || Convert.ToInt32(value) < peakLimit)
                            {
                                sum += value;
                                count++;
                            }
                        }
                    }
                }
            }

            return count > 0 ? sum / count : 0;
        }

        public static double CalculateMinimum(List<BaseCsvData> list, string dataSelection)
        {
            if (list.Count == 0) return double.MaxValue;

            return list.Where(item =>
            {
                var propertyInfo = typeof(BaseCsvData).GetProperty(dataSelection);
                if (propertyInfo != null)
                {
                    object propertyValue = propertyInfo.GetValue(item, null);
                    return propertyValue != null && (propertyValue is double || propertyValue is int || propertyValue is float);
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

        public static double CalculateMaximum(List<BaseCsvData> list, string dataSelection, bool peakNormalization = false, int peakLimit = int.MaxValue)
        {
            if (list.Count == 0) return 0;

            return list.Max(item =>
            {
                var propertyInfo = typeof(BaseCsvData).GetProperty(dataSelection);
                if (propertyInfo != null)
                {
                    object propertyValue = propertyInfo.GetValue(item, null);
                    if (propertyValue is double || propertyValue is int || propertyValue is float)
                    {
                        var value = Convert.ToDouble(propertyValue);
                        if (!peakNormalization || Convert.ToInt32(value) < peakLimit)
                        {
                            return value;
                        }
                    }
                }
                return 0;
            });
        }

        public static string FormatNumberWithUnit(double number, string unit, bool showDecimalsOnlyIfNonZero = true)
        {
            string formattedNumber;

            if (showDecimalsOnlyIfNonZero && Math.Abs(number % 1) < double.Epsilon)
            {
                formattedNumber = number.ToString("F0");
            }
            else
            {
                formattedNumber = number.ToString("F2");
            }

            if (formattedNumber.StartsWith("- "))
            {
                formattedNumber = "-" + formattedNumber.Substring(2);
            }

            return formattedNumber + unit;
        }

        public static (string Green, string Red, string Blue) CalculateCardValues(
        List<BaseCsvData> inputDataList,
        string dataSelection,
        string unit,
        bool peakSmooth = false,
        int peakUpperLimit = int.MaxValue)
        {
            bool isPing = dataSelection == "Ping";

            var maxValue = CalculateMaximum(inputDataList, dataSelection, peakSmooth, peakUpperLimit);
            var minValue = CalculateMinimum(inputDataList, dataSelection);

            string greenCardValue = FormatNumberWithUnit(isPing ? minValue : maxValue, unit);
            string redCardValue = FormatNumberWithUnit(isPing ? maxValue : minValue, unit);
            string blueCardValue;

            switch (dataSelection)
            {
                case "PCI":
                    blueCardValue = "N/A";
                    break;

                case "CQI":
                    var avgValue = Math.Floor(CalculateAverage(inputDataList, dataSelection, peakSmooth, peakUpperLimit));
                    blueCardValue = FormatNumberWithUnit(avgValue, unit);
                    break;

                default:
                    var avg = CalculateAverage(inputDataList, dataSelection, peakSmooth, peakUpperLimit);
                    blueCardValue = FormatNumberWithUnit(avg, unit);
                    break;
            }

            return (greenCardValue, redCardValue, blueCardValue);
        }
    }
}
