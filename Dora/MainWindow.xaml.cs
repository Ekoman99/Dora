using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using CsvHelper;
using CsvHelper.Configuration;
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
using CsvHelper.Configuration;
using System.Globalization;

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

            if (dataLoadedCSV == true) 
            {
                List<CsvData> csvDataList = LoadDataFromCsv(FilePath);
            }
        }

        bool dataLoadedCSV = false;
        public string FilePath
        {           
            get { return (string)GetValue(filePathCSVProperty); }
            set { SetValue(filePathCSVProperty, value); }
        }

        public static readonly DependencyProperty filePathCSVProperty =
            DependencyProperty.Register("FilePath", typeof(string), typeof(MainWindow), new PropertyMetadata(string.Empty));

        private void CSV_File_Selection(object sender, RoutedEventArgs e)
        {
            // Here you can implement your logic to load the CSV file and extract the latitude and longitude points
            {
                Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

                Nullable<bool> result = dlg.ShowDialog();

                if (result == true)
                {
                    FilePath = dlg.FileName;
                    dataLoadedCSV = true;
                }
            }

        }

        public class CsvData
        {
            public float RSRP { get; set; }
        }
        List<CsvData> LoadDataFromCsv(string filePath)
        {
            List<CsvData> dataList = new List<CsvData>();

            // Configure CsvHelper to use the appropriate delimiter (comma by default) and handle floats.
            var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture);
            csvConfig.HasHeaderRecord = true; // Assumes the first row contains headers.

            using (var reader = new StreamReader(filePath))
            using (var csv = new CsvReader(reader, csvConfig))
            {
                dataList = csv.GetRecords<CsvData>().ToList();
            }

            return dataList;
        }
    }
}
