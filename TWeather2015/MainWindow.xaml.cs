using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace TWeather2015
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /*
        План по активации и деактивации:
        1-1.    Определяем текущую структуру окон.
        1-2.    Если эта программа уже запущена не запускаем вторую.
        1-3.    Убираем существующий рабочий стол.
        1-4.    Ставим новый рабочий стол
        2.      Занимемся тем что должен делать рабочий стол :)
        3-1.    Убираем новый рабочий стол.
        3-2.    Возвращаем прежний рабочий стол.
        3-3.    Выключаемся.
        */
        IntPtr handle = IntPtr.Zero;
        TestWindow tw = new TestWindow();
        public MainWindow()
        {
            InitializeComponent();
            tw.Show();
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            /* base.OnSourceInitialized(e);
             HwndSource source = PresentationSource.FromVisual(this) as HwndSource;
             source.AddHook(WndProc);*/
        }

        


        public void SetBottom(IntPtr after)
        {
            if (handle == IntPtr.Zero) handle = new WindowInteropHelper(this).Handle;
            SetWindowPos(handle, after, 0, 0, 0, 0, SWP_NOSIZE | SWP_NOMOVE | SWP_NOACTIVATE);
            Console.WriteLine("SetButtom after {0}", after);
        }

        [DllImport("user32.dll")]
        static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        const UInt32 SWP_NOSIZE = 0x0001;
        const UInt32 SWP_NOMOVE = 0x0002;
        const UInt32 SWP_NOACTIVATE = 0x0010;
        const UInt32 SWP_NOZORDER = 0x0004;
        const int WM_WINDOWPOSCHANGING = 0x0046;
        const int WM_ACTIVATE = 0x0006;
        const int WM_MOVE = 0x0003;

        static readonly IntPtr HWND_BOTTOM = new IntPtr(1);
        [StructLayout(LayoutKind.Sequential)]
        public struct WINDOWPOS
        {
            public IntPtr hwnd;
            public IntPtr hwndInsertAfter;
            public int x;
            public int y;
            public int cx;
            public int cy;
            public uint flags;
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            if (WindowState != WindowState.Normal)
                WindowState = WindowState.Normal;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (handle == IntPtr.Zero) handle = new WindowInteropHelper(this).Handle;
            IntPtr hwndf = handle;
            IntPtr hwndParent = FindWindow("ProgMan", null);
            IntPtr next = FindWindowEx(hwndParent, IntPtr.Zero, "SHELLDLL_DefView", null);
            SetParent(tw.handle(), next);
            SetParent(hwndf, next);
            next = FindWindowEx(next, IntPtr.Zero, "SysListView32", "FolderView");
            {
                var i = GetWindowLong(next, -16);
                if ((i & 0x04000000L) > 0) i -= 0x04000000;
                Console.WriteLine(i);
                SetWindowLong(next, -16, i);
            }
            SetParent(next, hwndf);
            //ShowWindow(next, 1);
            //SetBottom(next);
            this.Topmost = true;
        }

        [DllImport("User32")]
        private static extern int ShowWindow(IntPtr hwnd, int nCmdShow);

        [DllImport("user32.dll")]
        static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
        [DllImport("user32.dll")]
        static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr FindWindow(
  [MarshalAs(UnmanagedType.LPTStr)] string lpClassName,
  [MarshalAs(UnmanagedType.LPTStr)] string lpWindowName);
        [DllImport("user32.dll")]
        public static extern IntPtr SetParent(
          IntPtr hWndChild,      // handle to window
          IntPtr hWndNewParent   // new parent window
          );
    }
}