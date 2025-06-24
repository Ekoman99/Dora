using System;
using System.Collections.Generic;
using System.Linq;

namespace Dora.Data
{
    static class MathEngine
    {
        /// <summary>
        /// Funkcija za računanje prosjeka, ukoliko se pojave abnormalno veliki brojevi (poput praznog registra) oni se ne uzimaju u prosjek. Odabir parametra vrši se pomoći stringa dataSelection
        /// </summary>
        /// <param name="list"></param>
        /// <param name="dataSelection"></param>
        /// <param name="peakNormalization"></param>
        /// <param name="peakLimit"></param>
        /// <returns></returns>
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
                        object dataSelValue = propertyInfo.GetValue(item, null);
                        if (dataSelValue != null && (dataSelValue is double || dataSelValue is int || dataSelValue is float)) // ako nije nula i ako je broj
                        {
                            var finalValue = Convert.ToDouble(dataSelValue);
                            if (!peakNormalization || Convert.ToInt32(finalValue) < peakLimit)
                            {
                                sum += finalValue;
                                count++;
                            }
                        }
                    }
                }
            }

            return count > 0 ? sum / count : 0;
        }

        /// <summary>
        /// Funkcija za pronalazak minimuma određenog parametra zadanog pomoću stringa dataSelection
        /// </summary>
        /// <param name="list"></param>
        /// <param name="dataSelection"></param>
        /// <returns></returns>
        public static double CalculateMinimum(List<BaseCsvData> list, string dataSelection)
        {
            if (list.Count == 0) return double.MaxValue;

            return list.Where(item =>
            {
                var propertyInfo = typeof(BaseCsvData).GetProperty(dataSelection);
                if (propertyInfo != null)
                {
                    object dataSelValue = propertyInfo.GetValue(item, null);
                    return dataSelValue != null && (dataSelValue is double || dataSelValue is int || dataSelValue is float);
                }
                return false;
            })
            .Min(item =>
            {
                var propertyInfo = typeof(BaseCsvData).GetProperty(dataSelection);
                object dataSelValue = propertyInfo.GetValue(item, null);
                return Convert.ToDouble(dataSelValue);
            });
        }

        /// <summary>
        /// Funkcija za pronalazak maksimuma određenog parametra zadanog pomoću stringa dataSelection. Maksimalna vrijednost registra biti će zanemarena.
        /// </summary>
        /// <param name="list"></param>
        /// <param name="dataSelection"></param>
        /// <param name="peakNormalization"></param>
        /// <param name="peakLimit"></param>
        /// <returns></returns>
        public static double CalculateMaximum(List<BaseCsvData> list, string dataSelection, bool peakNormalization = false, int peakLimit = int.MaxValue)
        {
            if (list.Count == 0) return 0;

            return list.Max(item =>
            {
                var propertyInfo = typeof(BaseCsvData).GetProperty(dataSelection);
                if (propertyInfo != null)
                {
                    object dataSelValue = propertyInfo.GetValue(item, null);
                    if (dataSelValue is double || dataSelValue is int || dataSelValue is float)
                    {
                        var value = Convert.ToDouble(dataSelValue);
                        if (!peakNormalization || Convert.ToInt32(value) < peakLimit)
                        {
                            return value;
                        }
                    }
                }
                return 0;
            });
        }

        /// <summary>
        /// Funckija koja popravlja format broja za ljepšu prezentaciju. Ukoliko broj nema decimalni dio on se briše, inače se prikažu dvije decimale. Uklanja se razmak između negativnog predznaka i broja.
        /// </summary>
        /// <param name="number"></param>
        /// <param name="unit"></param>
        /// <param name="decimalsNonZero"></param>
        /// <returns></returns>
        public static string FixFormat(double number, string unit, bool decimalsNonZero = true)
        {
            string formattedNumber;

            if (decimalsNonZero && Math.Abs(number % 1) < double.Epsilon) //epsilon je najmanja moguća decimalna vrijednost double registra
            {
                formattedNumber = number.ToString("F0");
            }
            else
            {
                formattedNumber = number.ToString("F2");
            }

            // Ukoliko je broj negativan, ramak između minusa i broja će se ukloniti
            if (formattedNumber.StartsWith("- "))
            {
                formattedNumber = "-" + formattedNumber.Substring(2);
            }

            return formattedNumber + unit;
        }

        /// <summary>
        /// Funkcija za računanje minimuma, prosjeka i maksimuma za popunjavanje pripadajućih kartica u korisničkom sučelju
        /// </summary>
        /// <param name="inputDataList"></param>
        /// <param name="dataSelection"></param>
        /// <param name="unit"></param>
        /// <param name="peakSmooth"></param>
        /// <param name="peakUpperLimit"></param>
        /// <returns></returns>
        public static (string Green, string Red, string Blue) CalculateCardValues(List<BaseCsvData> inputDataList, string dataSelection, string unit, bool peakSmooth = false, int peakUpperLimit = int.MaxValue)
        {
            bool isPing = dataSelection == "Ping";

            var maxValue = CalculateMaximum(inputDataList, dataSelection, peakSmooth, peakUpperLimit);
            var minValue = CalculateMinimum(inputDataList, dataSelection);

            // ako se radi o pingu tad je najmanja vrijednost zelena, inače je obrnuto
            string greenCardValue = FixFormat(isPing ? minValue : maxValue, unit);
            string redCardValue = FixFormat(isPing ? maxValue : minValue, unit);
            string blueCardValue;

            // PCI prosjek nema smisla, CQI se zaokružuje na cijeli broj
            switch (dataSelection)
            {
                case "PCI":
                    blueCardValue = "N/A";
                    break;

                case "CQI":
                    var avgValue = Math.Floor(CalculateAverage(inputDataList, dataSelection, peakSmooth, peakUpperLimit));
                    blueCardValue = FixFormat(avgValue, unit);
                    break;

                default:
                    var avg = CalculateAverage(inputDataList, dataSelection, peakSmooth, peakUpperLimit);
                    blueCardValue = FixFormat(avg, unit);
                    break;
            }

            return (greenCardValue, redCardValue, blueCardValue);
        }
    }
}
