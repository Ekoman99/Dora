using Dora.Data;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Forms;

namespace Dora
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        private Dictionary<string, List<MapColorIntervals>> intervals;
        private string settingsPath;
        private string selectedParameter;

        public SettingsWindow(Dictionary<string, List<MapColorIntervals>> dataIntervals, string settingsPath)
        {
            InitializeComponent();
            intervals = new Dictionary<string, List<MapColorIntervals>>(dataIntervals);
            this.settingsPath = settingsPath;

            // combo za parametre
            ParameterComboBox.ItemsSource = intervals.Keys;
            if (intervals.Keys.Count > 0)
            {
                ParameterComboBox.SelectedIndex = 0;
            }
        }

        private void ParameterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selectedParameter = ParameterComboBox.SelectedItem as string;
            if (selectedParameter != null)
            {
                LoadIntervals(selectedParameter);
            }
        }

        private void LoadIntervals(string parameter)
        {
            if (intervals.ContainsKey(parameter))
            {
                var intervalsList = intervals[parameter].Select(interval => new MapColorIntervals
                {
                    Id = interval.Id,
                    LowerLimit = interval.LowerLimit,
                    UpperLimit = interval.UpperLimit,
                    Color = interval.Color
                }).ToList();

                IntervalsGrid.ItemsSource = intervalsList;
            }
        }

        private void SaveButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                List<MapColorIntervals> list = new List<MapColorIntervals>();
                foreach (var item in IntervalsGrid.Items)
                {
                    list.Add((MapColorIntervals)item);
                }

                // upozorenje
                if (!ValidateIntervals(list))
                {
                    System.Windows.MessageBox.Show("Invalid intervals. Please ensure:\n" +
                                  "- All fields are filled\n" +
                                  "- Intervals are continuous\n" +
                                  "- Lower limits are less than upper limits",
                                  "Validation Error",
                                  MessageBoxButton.OK,
                                  MessageBoxImage.Warning);
                    return;
                }

                intervals[selectedParameter] = list;

                // spremanje u JSON
                string json = JsonConvert.SerializeObject(intervals, Formatting.Indented);
                File.WriteAllText(settingsPath, json);

                System.Windows.MessageBox.Show("Settings saved successfully!", "Success",
                              MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error saving settings: {ex.Message}", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool ValidateIntervals(List<MapColorIntervals> intervals)
        {
            if (intervals == null || !intervals.Any())
                return false;

            var sortedIntervals = intervals.OrderBy(i => i.LowerLimit).ToList();

            for (int i = 0; i < sortedIntervals.Count; i++)
            {
                var interval = sortedIntervals[i];

                if (string.IsNullOrEmpty(interval.Color))
                    return false;

                if (interval.LowerLimit >= interval.UpperLimit)
                    return false;

                if (i < sortedIntervals.Count - 1)
                {
                    var nextInterval = sortedIntervals[i + 1];
                    if (interval.UpperLimit != nextInterval.LowerLimit)
                        return false;
                }
            }

            return true;
        }

        private void ColorPicker_Click(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.Button button && button.Tag is MapColorIntervals interval)
            {
                Color currentColor = Colors.Black;
                if (!string.IsNullOrEmpty(interval.Color))
                {
                    try
                    {
                        currentColor = (Color)ColorConverter.ConvertFromString(interval.Color);
                    }
                    catch { }
                }

                var dialog = new ColorDialog
                {
                    Color = System.Drawing.Color.FromArgb(currentColor.R, currentColor.G, currentColor.B),
                    FullOpen = true
                };

                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    interval.Color = $"#{dialog.Color.R:X2}{dialog.Color.G:X2}{dialog.Color.B:X2}"; //string

                    CollectionViewSource.GetDefaultView(IntervalsGrid.ItemsSource).Refresh();
                }
            }
        }

        private void CancelButtonClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }

    public class HexToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string hexColor)
            {
                try
                {
                    return (Color)ColorConverter.ConvertFromString(hexColor);
                }
                catch
                {
                    return Colors.Black;
                }
            }
            return Colors.Black;


        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Color color)
            {
                return $"#{color.R:X2}{color.G:X2}{color.B:X2}";
            }
            return "#000000";
        }
    }
}
