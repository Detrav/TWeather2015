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
        public string text { get { return textBlock.Text; } set { textBlock.Text = value; borderMain.ToolTip = value; } }
        public string filename { get; private set; }

        /*#region Properties
        public static readonly DependencyProperty SelectedProperty = DependencyProperty.Register("isSelected", typeof(bool), typeof(DIcon));
        public bool isSelected
        {
            get { return (true.Equals(GetValue(SelectedProperty))); }
            set { SetValue(SelectedProperty, value); }
        }
        #endregion Properties*/
        private bool isSelected = false;
        public bool IsSelected
        {
            get { return isSelected; }
            set
            {
                isSelected = value;
                borderMain.Background = (value) ? select : none;
                borderMain.BorderBrush = (value) ? SystemColors.HighlightBrush : none;
                textBlock.TextTrimming = (value) ? TextTrimming.None : TextTrimming.CharacterEllipsis;
            }
        }

        static Brush none = new SolidColorBrush(Colors.Transparent);
        static Brush select = new SolidColorBrush() { Color = SystemColors.ActiveCaptionColor, Opacity = 0.2  };

        public DIconManager myParent { get; private set; }

        public DIcon()
        {
            InitializeComponent();
        }

        public DIcon(DIconManager myParent,string filename, int x, int y) : this()
        {
            setPosition(x, y);
            text = System.IO.Path.GetFileNameWithoutExtension(filename);
            Console.WriteLine("{0} {1} {2}", text, x, y);
            this.filename = filename;
            this.myParent = myParent;
        }

        public void setPosition(int x, int y)
        {
            this.x = x;
            this.y = y;
            SetValue(Grid.ColumnProperty, x);
            SetValue(Grid.RowProperty, y);
        }

        private void borderMain_Click(object sender, RoutedEventArgs e)
        {
           // myParent.selectAll(false);
           // IsSelected = true;
        }

       internal bool inRect(double left, double top, double right, double bottom)
        {
            Point p = borderMain.TransformToAncestor(myParent).Transform(new Point(0, 0));
            if (p.X + borderMain.ActualWidth < left) return false;
            if (p.X > right) return false;
            if (p.Y > bottom) return false;
            if (p.Y + borderMain.ActualHeight < top) return false;
            return true;
        }

        private void borderMain_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            myParent.DIcon_PreviewMouseDown(this, e);
            //Console.WriteLine(e.GetPosition(null).Y);
        }
    }
}
