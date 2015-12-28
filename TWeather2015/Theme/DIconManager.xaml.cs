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
    /// Логика взаимодействия для DIconManager.xaml
    /// </summary>
    public partial class DIconManager : UserControl
    {
        public DIconManager()
        {
            InitializeComponent();
        }

        public int wcount { get; private set; }
        public int hcount { get; private set; }

        public DIcon[,] items { get; private set; }

        public int reCreateGrid(double w, double h)
        {
            wcount = (int)(w / 76);
            hcount = (int)(h / 100);
            //Console.WriteLine("{0} {1}", wcount, hcount);
            GridLength wlength = new GridLength(76);
            GridLength hlength = new GridLength(100);

            gridMain.ColumnDefinitions.Clear();
            gridMain.RowDefinitions.Clear();

            for (int i = 0; i < wcount; i++)
            {
                ColumnDefinition cd = new ColumnDefinition();
                cd.Width = wlength;
                gridMain.ColumnDefinitions.Add(cd);
            }
            gridMain.ColumnDefinitions.Add(new ColumnDefinition());
            for (int i = 0; i < hcount; i++)
            {
                RowDefinition rd = new RowDefinition();
                rd.Height = hlength;
                gridMain.RowDefinitions.Add(rd);
            }
            gridMain.RowDefinitions.Add(new RowDefinition());

            return wcount*hcount;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            (Parent as MainWindow).Close();
        }


        public DIcon addNewIcon(string filename)
        {
            for (int i = 0; i < wcount; i++)
                for (int j = 0; j < hcount; j++)
                {
                    if(items[i,j] == null)
                    {
                        items[i, j] = new DIcon(filename,i,j);
                        gridMain.Children.Add(items[i, j]);
                        return items[i,j];
                    }
                }
            return null;
        }

        public DIcon placeIcon(DIcon icon)
        {
            if (icon.x >= wcount || icon.y >= hcount || items[icon.x, icon.y] != null)
            {
                for (int i = 0; i < wcount; i++)
                    for (int j = 0; j < hcount; j++)
                    {
                        if (items[i, j] == null)
                        {
                            icon.setPosition(i, j);
                            items[icon.x, icon.y] = icon;
                            return icon;
                        }
                    }
                return null;
            }
            items[icon.x, icon.y] = icon;
            return icon;
        }

        public int reCalculateItems()
        {
            int count = 0;
            items = new DIcon[wcount, hcount];
            foreach(DIcon it in gridMain.Children)
            {
                placeIcon(it);
            }
            return count;
        }
        
    }
}
