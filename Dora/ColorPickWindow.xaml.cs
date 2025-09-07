using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using Newtonsoft.Json;
using System.Globalization;
using Dora.Data;

namespace Dora
{
    public partial class ColorSettingsWindow : Window
    {
        private SettingsDefinitions currentSettings;
        private SettingsDefinitions originalSettings;
        private SettingsDefinitions defaultSettings;
        private string settingsFilePath;
        private ObservableCollection<ColorItem> colorItems;

        public ColorSettingsWindow(SettingsDefinitions settings, string filePath)
        {
            InitializeComponent();
            currentSettings = settings;
            settingsFilePath = filePath;

            // kopija postavki
            originalSettings = JsonConvert.DeserializeObject<SettingsDefinitions>(JsonConvert.SerializeObject(settings));

            // default za reset
            defaultSettings = new SettingsDefinitions
            {
                NRColor = "#C41F1F",      // crvena
                LTEColor = "#349DC8",     // plava
                GraphBackground = "#00FFFFFF", // prozirno
                GraphElements = "#FFFFFF",
            };

            InitializeColorItems();
            ColorsGrid.ItemsSource = colorItems;
        }

        private void InitializeColorItems()
        {
            colorItems = new ObservableCollection<ColorItem>
            {
                new ColorItem { Name = "NR Color", ColorHex = currentSettings.NRColor, PropertyName = "NRColor" },
                new ColorItem { Name = "LTE Color", ColorHex = currentSettings.LTEColor, PropertyName = "LTEColor" },
                new ColorItem { Name = "Graph Background", ColorHex = currentSettings.GraphBackground, PropertyName = "GraphBackground" },
                new ColorItem { Name = "Graph Elements", ColorHex = currentSettings.GraphElements, PropertyName = "GraphElements"}
            };
        }

        private void ColorPicker_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var colorItem = button?.Tag as ColorItem;

            if (colorItem == null) return;

            var colorDialog = new System.Windows.Forms.ColorDialog();

            //kako bi automatski bio otvoren izbor boja

            colorDialog.FullOpen = true;

            // postavi boju
            try
            {
                var currentColor = (Color)ColorConverter.ConvertFromString(colorItem.ColorHex);
                colorDialog.Color = System.Drawing.Color.FromArgb(currentColor.A, currentColor.R, currentColor.G, currentColor.B);
            }
            catch { }

            if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                var selectedColor = colorDialog.Color;
                var hexColor = $"#{selectedColor.R:X2}{selectedColor.G:X2}{selectedColor.B:X2}";
                colorItem.ColorHex = hexColor;

                // update
                switch (colorItem.PropertyName)
                {
                    case "NRColor":
                        currentSettings.NRColor = hexColor;
                        break;
                    case "LTEColor":
                        currentSettings.LTEColor = hexColor;
                        break;
                    case "GraphBackground":
                        currentSettings.GraphBackground = hexColor;
                        break;
                    case "GraphElements":
                        currentSettings.GraphElements = hexColor;
                        break;
                }

                // refresh kolecije
                CollectionViewSource.GetDefaultView(ColorsGrid.ItemsSource).Refresh();
            }
        }

        private void ResetSingleColor_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var colorItem = button?.Tag as ColorItem;

            if (colorItem == null) return;

            string defaultValue = "";
            switch (colorItem.PropertyName)
            {
                case "NRColor":
                    defaultValue = defaultSettings.NRColor;
                    currentSettings.NRColor = defaultValue;
                    break;
                case "LTEColor":
                    defaultValue = defaultSettings.LTEColor;
                    currentSettings.LTEColor = defaultValue;
                    break;
                case "GraphBackground":
                    defaultValue = defaultSettings.GraphBackground;
                    currentSettings.GraphBackground = defaultValue;
                    break;
                case "GraphElements":
                    defaultValue = defaultSettings.GraphElements;
                    currentSettings.GraphElements = defaultValue;
                    break;
            }

            colorItem.ColorHex = defaultValue;

            // refresh kolekcije
            CollectionViewSource.GetDefaultView(ColorsGrid.ItemsSource).Refresh();
        }

        private void SaveButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                // spremanje u datoteku
                string json = JsonConvert.SerializeObject(currentSettings, Formatting.Indented);
                File.WriteAllText(settingsFilePath, json);

                MessageBox.Show("Color settings saved successfully!", "Success",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving settings: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ResetButtonClick(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Reset all colors to default values?", "Confirm Reset",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                // osvježavanje postavki
                currentSettings.NRColor = defaultSettings.NRColor;
                currentSettings.LTEColor = defaultSettings.LTEColor;
                currentSettings.GraphBackground = defaultSettings.GraphBackground;
                currentSettings.GraphElements = defaultSettings.GraphElements;

                // update kolecije
                foreach (var item in colorItems)
                {
                    switch (item.PropertyName)
                    {
                        case "NRColor":
                            item.ColorHex = defaultSettings.NRColor;
                            break;
                        case "LTEColor":
                            item.ColorHex = defaultSettings.LTEColor;
                            break;
                        case "GraphBackground":
                            item.ColorHex = defaultSettings.GraphBackground;
                            break;
                        case "GraphElements":
                            item.ColorHex = defaultSettings.GraphElements;
                            break;
                    }
                }

                // reset
                try
                {
                    string json = JsonConvert.SerializeObject(currentSettings, Formatting.Indented);
                    File.WriteAllText(settingsFilePath, json);

                    MessageBox.Show("Colors reset to default values and saved successfully!", "Reset Complete",
                        MessageBoxButton.OK, MessageBoxImage.Information);

                    this.DialogResult = true;
                    this.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error saving reset settings: {ex.Message}", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void CancelButtonClick(object sender, RoutedEventArgs e)
        {
            // originalne postavke
            currentSettings.NRColor = originalSettings.NRColor;
            currentSettings.LTEColor = originalSettings.LTEColor;
            currentSettings.GraphBackground = originalSettings.GraphBackground;
            currentSettings.GraphElements = originalSettings.GraphElements;

            this.DialogResult = false;
            this.Close();
        }
    }

    public class ColorItem : INotifyPropertyChanged
    {
        private string _colorHex;

        public string Name { get; set; }
        public string PropertyName { get; set; }

        public string ColorHex
        {
            get => _colorHex;
            set
            {
                if (_colorHex != value)
                {
                    _colorHex = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}