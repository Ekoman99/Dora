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
    /// Interaction logic for Login.xaml
    /// </summary>
    public partial class Login : Window
    {
        public Login()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string user = usernameInput.Text;
            string pass = passwordInput.Password;

            if (/*user == "admin" && pass == "admin"*/true)
            {
                MainWindow mWin = new MainWindow();
                mWin.Show();
                this.Close();
            }
            else { MessageBox.Show("Pogrešni podaci"); this.Close(); }
        }
    }
}
