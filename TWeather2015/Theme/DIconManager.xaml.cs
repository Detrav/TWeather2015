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

            items = new DIcon[wcount, hcount];
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

        bool mouseDowned = false;
        Point mouseDownPos; 

        private void gridMain_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                mouseDowned = false;
                
                selectionBox.Visibility = Visibility.Collapsed;
                Point mouseUpPos = e.GetPosition(gridMain);

                double left = Math.Min(mouseDownPos.X, mouseUpPos.X);
                double right = Math.Max(mouseDownPos.X, mouseUpPos.X);
                double top = Math.Min(mouseDownPos.Y, mouseUpPos.Y);
                double bottom = Math.Max(mouseDownPos.Y, mouseUpPos.Y);

                selectAll(true, left, top, right, bottom);
            }
            gridMain.ReleaseMouseCapture();
        }

        private void gridMain_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Console.WriteLine(e.ChangedButton);
            if (e.ChangedButton == MouseButton.Left)
            {

                mouseDownPos = e.GetPosition(gridMain);
                gridMain.CaptureMouse();
                Canvas.SetLeft(selectionBox, mouseDownPos.X);
                Canvas.SetTop(selectionBox, mouseDownPos.Y);
                selectionBox.Width = 0;
                selectionBox.Height = 0;
                selectionBox.Visibility = Visibility.Visible;
                mouseDowned = true;
            }
        }

        private void gridMain_MouseMove(object sender, MouseEventArgs e)
        {
            if(mouseDowned)
            {
                Point mousePos = e.GetPosition(gridMain);
                if (mouseDownPos.X < mousePos.X)
                {
                    Canvas.SetLeft(selectionBox, mouseDownPos.X);
                    selectionBox.Width = mousePos.X - mouseDownPos.X;
                }
                else
                {
                    Canvas.SetLeft(selectionBox, mousePos.X);
                    selectionBox.Width = mouseDownPos.X - mousePos.X;
                }

                if (mouseDownPos.Y < mousePos.Y)
                {
                    Canvas.SetTop(selectionBox, mouseDownPos.Y);
                    selectionBox.Height = mousePos.Y - mouseDownPos.Y;
                }
                else
                {
                    Canvas.SetTop(selectionBox, mousePos.Y);
                    selectionBox.Height = mouseDownPos.Y - mousePos.Y;
                }
            }
        }
    }
}
