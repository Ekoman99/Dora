using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dora.Data
{
    internal class Charting
    {
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
                                if (value >= interval.LowerLimit && value < interval.UpperLimit)
                                {
                                    color = interval.Color;
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
