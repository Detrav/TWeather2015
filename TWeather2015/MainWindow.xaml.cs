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
        IntPtr handle = IntPtr.Zero;

        public MainWindow()
        {
            InitializeComponent();

        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            /* base.OnSourceInitialized(e);
             HwndSource source = PresentationSource.FromVisual(this) as HwndSource;
             source.AddHook(WndProc);*/
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {

            switch (msg)
            {
                case WM_ACTIVATE:
                    SetBottom();
                    break;
                case WM_MOVE:
                    SetBottom();
                    break;
            }
            return IntPtr.Zero;
        }


        public void SetBottom()
        {
            if (handle == IntPtr.Zero) handle = new WindowInteropHelper(this).Handle;
            SetWindowPos(handle, HWND_BOTTOM, 0, 0, 0, 0, SWP_NOSIZE | SWP_NOMOVE | SWP_NOACTIVATE);
            Console.WriteLine("SetButtom");
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
            SetParent(hwndf, hwndParent);
            this.Topmost = false;
        }


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