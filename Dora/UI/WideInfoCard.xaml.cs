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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Dora.UI
{
    /// <summary>
    /// Interaction logic for WideInfoCard.xaml
    /// </summary>
    public partial class WideInfoCard : UserControl
    {
        public WideInfoCard()
        {
            InitializeComponent();
        }

        public string Title1
        {
            get { return (string)GetValue(Title1Property); }
            set { SetValue(Title1Property, value); }
        }

        public static readonly DependencyProperty Title1Property = DependencyProperty.Register("Title1", typeof(string), typeof(WideInfoCard));

        public string Number1
        {
            get { return (string)GetValue(Number1Property); }
            set { SetValue(Number1Property, value); }
        }

        public static readonly DependencyProperty Number1Property = DependencyProperty.Register("Number1", typeof(string), typeof(WideInfoCard));

        public string Title2
        {
            get { return (string)GetValue(Title2Property); }
            set { SetValue(Title2Property, value); }
        }

        public static readonly DependencyProperty Title2Property = DependencyProperty.Register("Title2", typeof(string), typeof(WideInfoCard));

        public string Number2
        {
            get { return (string)GetValue(Number2Property); }
            set { SetValue(Number2Property, value); }
        }

        public static readonly DependencyProperty Number2Property = DependencyProperty.Register("Number2", typeof(string), typeof(WideInfoCard));

        public string Title3
        {
            get { return (string)GetValue(Title3Property); }
            set { SetValue(Title3Property, value); }
        }

        public static readonly DependencyProperty Title3Property = DependencyProperty.Register("Title3", typeof(string), typeof(WideInfoCard));

        public string Number3
        {
            get { return (string)GetValue(Number3Property); }
            set { SetValue(Number3Property, value); }
        }

        public static readonly DependencyProperty Number3Property = DependencyProperty.Register("Number3", typeof(string), typeof(WideInfoCard));

        public string Title4
        {
            get { return (string)GetValue(Title4Property); }
            set { SetValue(Title4Property, value); }
        }

        public static readonly DependencyProperty Title4Property = DependencyProperty.Register("Title4", typeof(string), typeof(WideInfoCard));

        public string Number4
        {
            get { return (string)GetValue(Number4Property); }
            set { SetValue(Number4Property, value); }
        }

        public static readonly DependencyProperty Number4Property = DependencyProperty.Register("Number4", typeof(string), typeof(WideInfoCard));

        public string Percentage1
        {
            get { return (string)GetValue(Percentage1Property); }
            set { SetValue(Percentage1Property, value); }
        }

        public static readonly DependencyProperty Percentage1Property = DependencyProperty.Register("Percentage1", typeof(string), typeof(WideInfoCard));

        public string Percentage2
        {
            get { return (string)GetValue(Percentage2Property); }
            set { SetValue(Percentage2Property, value); }
        }

        public static readonly DependencyProperty Percentage2Property = DependencyProperty.Register("Percentage2", typeof(string), typeof(WideInfoCard));

        public string Percentage3
        {
            get { return (string)GetValue(Percentage3Property); }
            set { SetValue(Percentage3Property, value); }
        }

        public static readonly DependencyProperty Percentage3Property = DependencyProperty.Register("Percentage3", typeof(string), typeof(WideInfoCard));

        public string Percentage4
        {
            get { return (string)GetValue(Percentage4Property); }
            set { SetValue(Percentage4Property, value); }
        }

        public static readonly DependencyProperty Percentage4Property = DependencyProperty.Register("Percentage4", typeof(string), typeof(WideInfoCard));

        public FontAwesome.Sharp.IconChar Icon
        {
            get { return (FontAwesome.Sharp.IconChar)GetValue(IconProperty); }
            set { SetValue(IconProperty, value); }
        }

        public static readonly DependencyProperty IconProperty = DependencyProperty.Register("Icon", typeof(FontAwesome.Sharp.IconChar), typeof(WideInfoCard));

        public Color Background1
        {
            get { return (Color)GetValue(Background1Property); }
            set { SetValue(Background1Property, value); }
        }

        public static readonly DependencyProperty Background1Property = DependencyProperty.Register("Background1", typeof(Color), typeof(WideInfoCard));

        public Color Background2
        {
            get { return (Color)GetValue(Background2Property); }
            set { SetValue(Background2Property, value); }
        }

        public static readonly DependencyProperty Background2Property = DependencyProperty.Register("Background2", typeof(Color), typeof(WideInfoCard));

        public Color EllipseBackground1
        {
            get { return (Color)GetValue(EllipseBackground1Property); }
            set { SetValue(EllipseBackground1Property, value); }
        }

        public static readonly DependencyProperty EllipseBackground1Property = DependencyProperty.Register("EllipseBackground1", typeof(Color), typeof(WideInfoCard));

        public Color EllipseBackground2
        {
            get { return (Color)GetValue(EllipseBackground2Property); }
            set { SetValue(EllipseBackground2Property, value); }
        }

        public static readonly DependencyProperty EllipseBackground2Property = DependencyProperty.Register("EllipseBackground2", typeof(Color), typeof(WideInfoCard));

        //defaultne kartice za UI
        public static WideInfoCard WideCardDefault
        {
            get
            {
                return new WideInfoCard
                {
                    Title1 = "Maximum",                    
                    Title2 = "Average",
                    Title3 = "Minimum",
                    Title4 = "test",
                    Number1 = "testnum",
                    Number2 = "testnum2",
                    Number3 = "No file selected",
                    Number4 = "No file selected",
                    Percentage1 = "50%",
                    Percentage2 = "60%",
                    Percentage3 = "70%",
                    Percentage4 = "80%",
                    Icon = FontAwesome.Sharp.IconChar.PlusCircle,
                    Background1 = Color.FromArgb(255, 207, 162, 41),
                    Background2 = Color.FromArgb(255, 222, 180, 67),
                    EllipseBackground1 = Color.FromArgb(255, 165, 227, 69),
                    EllipseBackground2 = Color.FromArgb(255, 178, 238, 83)
                };
            }
        }
    }
}
