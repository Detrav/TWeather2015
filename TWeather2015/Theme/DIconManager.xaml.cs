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
            foreach (var i in Application.Current.Windows)
            {
                if (i is MainWindow)
                {
                    myParent = i as MainWindow;
                    break;
                }
            }
        }

        public MainWindow myParent { get; private set; }

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

            items = new DIcon[wcount, hcount];
            return wcount*hcount;
        }

        public DIcon addNewIcon(string filename)
        {
            for (int i = 0; i < wcount; i++)
                for (int j = 0; j < hcount; j++)
                {
                    if(items[i,j] == null)
                    {
                        items[i, j] = new DIcon(this,filename,i,j);
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

        internal void DIcon_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            myParent.DIcon_PreviewMouseDown(sender, e);
        }

        internal void selectAll(bool v)
        {
            foreach (DIcon it in gridMain.Children)
            {
                it.IsSelected = v;
            }
            //throw new NotImplementedException();
        }

        internal void selectAll(bool v, double left, double top,double right,double bottom)
        {
            foreach (DIcon it in gridMain.Children)
            {
                it.IsSelected = !v;
                if (it.inRect(left, top, right,bottom)) it.IsSelected = v;
            }
        }

        internal DataObject getDataForDragAndDrop()
        {
            List<string> files = new List<string>();
            foreach (DIcon it in gridMain.Children)
            {
                if(it.IsSelected)
                files.Add(it.filename);
            }
            if (files.Count == 0) return null;
            return new DataObject(DataFormats.FileDrop, files.ToArray(),true);
        }


    }
}
