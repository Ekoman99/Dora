using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dora.Data
{
    static class MathEngine
    {
        public static double CalculateAverage(List<BaseCsvData> list, string dataSelection)
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

        public static double CalculateAverage(List<BaseCsvData> list, string dataSelection, bool peakNormalization, int peakLimit)
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
                            if (peakNormalization == false)
                            {
                                sum += Convert.ToDouble(propertyValue);
                                count++;
                            }
                        }
                    }
                }
            }

            return count > 0 ? sum / count : 0; // div 0
        }

        public static double CalculateMinimum(List<BaseCsvData> list, string dataSelection)
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

        public static double CalculateMaximum(List<BaseCsvData> list, string dataSelection)
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

        public static double CalculateMaximum(List<BaseCsvData> list, string dataSelection, bool peakNormalization, int peakLimit)
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
    }
}
