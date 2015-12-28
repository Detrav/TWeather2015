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

namespace TWeather2015.Theme
{
    /// <summary>
    /// Логика взаимодействия для DIcon.xaml
    /// </summary>
    public partial class DIcon : UserControl
    {

        public int x { get; set; }
        public int y { get; set; }
        public string text { get { return textBlock.Text; } set { textBlock.Text = value; } }
        public string filename { get; private set; }
        

        public DIcon()
        {
            InitializeComponent();
        }

        public DIcon(string filename, int x, int y) : this()
        {
            setPosition(x, y);
            text = System.IO.Path.GetFileNameWithoutExtension(filename);
            Console.WriteLine("{0} {1} {2}", text, x, y);
            this.filename = filename;
        }

        public void setPosition(int x, int y)
        {
            this.x = x;
            this.y = y;
            SetValue(Grid.ColumnProperty, x);
            SetValue(Grid.RowProperty, y);
        }
    }
}
