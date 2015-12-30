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
using TWeather2015.Core;

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
            hcount = (int)(h / 104);
            //Console.WriteLine("{0} {1}", wcount, hcount);
            GridLength wlength = new GridLength(76);
            GridLength hlength = new GridLength(104);

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

        public void loadIcons(string[] files)
        {
            DIconPositionManager.loadPositions();
            foreach (var f in files)
            {
                var p = DIconPositionManager.getPosition(f);
                if (p.x == 0 && p.y == 0)
                    addNewIconToEnd(f);
                else
                {
                    gridMain.Children.Add(placeIcon(new DIcon(this, f, p.x, p.y)));
                }
            }
            DIconPositionManager.savePositions();
        }

        public DIcon addNewIconToEnd(string filename)
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
            if (icon.x < wcount && icon.x >= 0 && icon.y < hcount && icon.y >= 0)
                if (items[icon.x, icon.y] != null) if (items[icon.x, icon.y].filename == icon.filename) return items[icon.x, icon.y];
            var position = getNearestSpace(icon.x, icon.y);
            //Console.WriteLine("empty x:{0}-y:{1}", position.x, position.y);
            if (position == null) return null;
            icon.setPosition(position.x, position.y);
            items[icon.x, icon.y] = icon;
            return icon;
        }

        public int reCalculateItems()
        {
            int count = 0;
            items = new DIcon[wcount, hcount];
            foreach(DIcon it in gridMain.Children)
            {
                it.updateIcon();
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

        internal void DIcon_PreviewMouseUp(DIcon dIcon, MouseButtonEventArgs e)
        {
            List<string> strs = new List<string>();
            foreach(DIcon it in gridMain.Children)
            {
                if (it.IsSelected) strs.Add(it.filename);
            }
            myParent.DIcon_PreviewMouseUp(strs.ToArray(), e);
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

        internal void select(DIcon dIconTemp)
        {
            selectAll(false);
            dIconTemp.IsSelected = true;
        }

        internal void selectAdd(DIcon dIconTemp)
        {
            dIconTemp.IsSelected = true;
        }

        internal void selectAllAdd(bool v, double left, double top, double right, double bottom)
        {
            foreach (DIcon it in gridMain.Children)
            {
                if (it.inRect(left, top, right, bottom)) it.IsSelected = v;
            }
        }

        internal void prepareForDragAndDrop(DIcon dIconTemp)
        {
            if(!dIconTemp.IsSelected)
            {
                select(dIconTemp);
            }
            //D&D
        }

        private DIconPosition getNearestSpace(int x,int y)
        {
            if (x >= wcount || y >= hcount || items[x, y] != null)
            {
                //Первый цикл будет сдвигать влево вверх
                //Второй цикл будет сдвигать враво вниз
                //Грубо но думаю не будет слишком сложно, сложность будет 1,9,25...(1+2n)^n, даже если это 25 попыток с угла это всего 23 426 итераций, должно проскочить :)
                int left = x;
                int top = y;
                int right = x+1;
                int bottom = y+1;
                //Обезопасим shell :) без while(true)
                for (int count = 0; count < 25; count++)
                {
                    left--; top--; right++; bottom++;
                    if (left < 0) left = 0;
                    if (top < 0) top = 0;
                    if (right > wcount) right = wcount;
                    if (bottom > hcount) bottom = hcount;

                    for (int i = left; i < right; i++)
                        for (int j = top; j < bottom; j++)
                        { if (items[i, j] == null) return new DIconPosition(i, j); }
                    if (left == 0 && top == 0 && right == wcount && bottom == hcount) return null;
                }
                /*
                $$$$$$$
                $#####$
                $#***#$
                $#*+*#$
                $#***#$
                $#####$
                $$$$$$$
                */
            }
            return new DIconPosition(x,y);
        }

        internal void delete(string fullPath)
        {
            this.Dispatcher.Invoke((Action)(() =>
            {
                DIconPositionManager.deletePosition(fullPath);
                int temp = -1;

                for(int i = 0; i< gridMain.Children.Count;i++)
                {
                    if ((gridMain.Children[i] as DIcon).filename == fullPath)
                    {
                        temp = i;
                        break;
                    }
                }
                if (temp >= 0)
                {
                    DIcon deleted = gridMain.Children[temp] as DIcon;
                    gridMain.Children.RemoveAt(temp);
                    items[deleted.x, deleted.y] = null;
                }
                DIconPositionManager.savePositions();
            }));
        }

        internal void reName(string oldFullPath, string fullPath)
        {
            this.Dispatcher.Invoke((Action)(() =>
            {
                DIconPositionManager.renamePosition(oldFullPath, fullPath);
                foreach (DIcon it in gridMain.Children)
                {
                    if (it.filename == oldFullPath)
                    {
                        it.setFileName(fullPath);
                        break;
                    }
                }
                DIconPositionManager.savePositions();
            }));
        }

        internal void addNewIcon(string fullPath)
        {
            this.Dispatcher.Invoke((Action)(() =>
            {
                var p = DIconPositionManager.getPosition(fullPath);
                if (p.x == 0 && p.y == 0)
                    addNewIconToEnd(fullPath);
                else
                {
                    gridMain.Children.Add(placeIcon(new DIcon(this, fullPath, p.x, p.y)));
                }
                DIconPositionManager.savePositions();
            }));
        }

        internal void moveTo(string[] fileList, int x, int y)
        {
            foreach(var f in fileList)
            {
                foreach(DIcon it in gridMain.Children)
                {
                    if(it.filename == f)
                    {
                        items[it.x, it.y] = null;
                        it.setPosition(x, y);
                        placeIcon(it);
                    }
                }
            }
            DIconPositionManager.savePositions();
        }

        internal void updateIcon(string fullPath)
        {
            this.Dispatcher.Invoke((Action)(() =>
            {
                foreach (DIcon it in gridMain.Children)
                {
                    if (it.filename == fullPath)
                    {
                        it.updateIcon();
                        return;
                    }
                }
            }));
        }

        internal void sortByName()
        {
            SortedList<string, DIcon> list = new SortedList<string, DIcon>(new DuplicateKeyComparer<string>());
            foreach (DIcon it in gridMain.Children)
            {
                list.Add(System.IO.Path.GetFileName(it.filename), it);
            }
            DIcon[] items = list.Values.ToArray();
            int k = 0;
            for(int i = 0; i<wcount; i++)
                for(int j = 0; j< hcount; j++)
                {
                    if(k<items.Length)
                    {
                        items[k].setPosition(i, j);
                    }
                    else
                    {
                        DIconPositionManager.savePositions();
                        reCalculateItems();
                        return;
                    }
                    k++;
                }
        }

        /// <summary>
        /// Comparer for comparing two keys, handling equality as beeing greater
        /// Use this Comparer e.g. with SortedLists or SortedDictionaries, that don't allow duplicate keys
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        public class DuplicateKeyComparer<TKey>
                        :
                     IComparer<TKey> where TKey : IComparable
        {
            #region IComparer<TKey> Members

            public int Compare(TKey x, TKey y)
            {
                int result = x.CompareTo(y);

                if (result == 0)
                    return 1;   // Handle equality as beeing greater
                else
                    return result;
            }

            #endregion
        }
    }
}
