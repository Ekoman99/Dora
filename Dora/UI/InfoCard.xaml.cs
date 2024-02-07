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
    /// Interaction logic for InfoCard.xaml
    /// </summary>
    public partial class InfoCard : UserControl
    {
        public InfoCard()
        {
            InitializeComponent();
        }

        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register("Title", typeof(string), typeof(InfoCard));

        public string Number
        {
            get { return (string)GetValue(NumberProperty); }
            set { SetValue(NumberProperty, value); }
        }

        public static readonly DependencyProperty NumberProperty = DependencyProperty.Register("Number", typeof(string), typeof(InfoCard));

        public FontAwesome.Sharp.IconChar Icon
        {
            get { return (FontAwesome.Sharp.IconChar)GetValue(IconProperty); }
            set { SetValue(IconProperty, value); }
        }

        public static readonly DependencyProperty IconProperty = DependencyProperty.Register("Icon", typeof(FontAwesome.Sharp.IconChar), typeof(InfoCard));

        public Color Background1
        {
            get { return (Color)GetValue(Background1Property); }
            set { SetValue(Background1Property, value); }
        }

        public static readonly DependencyProperty Background1Property = DependencyProperty.Register("Background1", typeof(Color), typeof(InfoCard));

        public Color Background2
        {
            get { return (Color)GetValue(Background2Property); }
            set { SetValue(Background2Property, value); }
        }

        public static readonly DependencyProperty Background2Property = DependencyProperty.Register("Background2", typeof(Color), typeof(InfoCard));

        public Color EllipseBackground1
        {
            get { return (Color)GetValue(EllipseBackground1Property); }
            set { SetValue(EllipseBackground1Property, value); }
        }

        public static readonly DependencyProperty EllipseBackground1Property = DependencyProperty.Register("EllipseBackground1", typeof(Color), typeof(InfoCard));

        public Color EllipseBackground2
        {
            get { return (Color)GetValue(EllipseBackground2Property); }
            set { SetValue(EllipseBackground2Property, value); }
        }

        public static readonly DependencyProperty EllipseBackground2Property = DependencyProperty.Register("EllipseBackground2", typeof(Color), typeof(InfoCard));

        //defaultne kartice za UI
        public static InfoCard GreenCardDefault
        {
            get
            {
                return new InfoCard
                {
                    Title = "Maximum",
                    Number = "No file selected",
                    Icon = FontAwesome.Sharp.IconChar.PlusCircle,
                    Background1 = Color.FromArgb(255, 92, 196, 89),
                    Background2 = Color.FromArgb(255, 129, 204, 83),
                    EllipseBackground1 = Color.FromArgb(255, 165, 227, 69),
                    EllipseBackground2 = Color.FromArgb(255, 178, 238, 83)
                };
            }
        }

        public static InfoCard BlueCardDefault
        {
            get
            {
                return new InfoCard
                {
                    Title = "Average",
                    Number = "No file selected",
                    Icon = FontAwesome.Sharp.IconChar.ArrowsLeftRight,
                    Background1 = Color.FromArgb(255, 59, 165, 208),
                    Background2 = Color.FromArgb(255, 52, 157, 200),
                    EllipseBackground1 = Color.FromArgb(255, 82, 194, 222),
                    EllipseBackground2 = Color.FromArgb(255, 94, 208, 236)
                };
            }
        }

        public static InfoCard RedCardDefault
        {
            get
            {
                return new InfoCard
                {
                    Title = "Minimum",
                    Number = "No file selected",
                    Icon = FontAwesome.Sharp.IconChar.MinusCircle,
                    Background1 = Color.FromArgb(255, 197, 72, 72),
                    Background2 = Color.FromArgb(255, 196, 31, 31),
                    EllipseBackground1 = Color.FromArgb(255, 247, 85, 85),
                    EllipseBackground2 = Color.FromArgb(255, 248, 39, 39)
                };
            }
        }
    }
}
