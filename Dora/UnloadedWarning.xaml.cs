using System;
using System.Collections.Generic;
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

namespace Dora
{
    /// <summary>
    /// Interaction logic for UnloadedWarning.xaml
    /// </summary>
    public partial class UnloadedWarning : Window
    {
        public UnloadedWarning()
        {
            InitializeComponent();
        }

        private void confirmWarningClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
