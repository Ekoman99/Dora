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
                MainWindow mainWin = new MainWindow();
                mainWin.Show();
                this.Close();
            }
            else
            {
                MessageBox.Show("Pogrešni podaci"); 
                this.Close();
            }
        }
        private void UsernameboxChanged(object sender, TextChangedEventArgs e)
        {
            if (usernameInput.Text.Length > 0)
            {
                tbUsername.Visibility = Visibility.Collapsed;
            }

            else
            {
                tbUsername.Visibility = Visibility.Visible;
            }
                
        }

        private void PasswordboxChanged(object sender, RoutedEventArgs e)
        {
            if (passwordInput.Password.Length > 0)
            {
                tbPassword.Visibility = Visibility.Collapsed;
            }

            else
            {
                tbPassword.Visibility = Visibility.Visible;
            }
                
        }
    }
}
