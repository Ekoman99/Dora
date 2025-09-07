using CsvHelper.Configuration;
using CsvHelper;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Dora.MainWindow;

namespace Dora.Data
{
    internal class CSVHandler
    {
        public List<BaseCsvData> LoadCSV(string filePath)
        {
            List<BaseCsvData> dataList = new List<BaseCsvData>();

            var commaDecimal = new CultureInfo("hr-HR"); // zbog decimalnog zareza u hrv

            var csvConfig = new CsvConfiguration(commaDecimal);
            csvConfig.Delimiter = ";";
            csvConfig.HasHeaderRecord = true;

            using (var reader = new StreamReader(filePath))
            using (var csv = new CsvReader(reader, csvConfig))
            {
                csv.Context.TypeConverterCache.AddConverter<int?>(new NullableIntTypeConverter());
                dataList = csv.GetRecords<BaseCsvData>().ToList();
            }

            return dataList;
        }

    }
}
