using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.IO;


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
        IntPtr desktopHandle = IntPtr.Zero;

        FileSystemWatcher fswDesktop = new FileSystemWatcher(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory));
        FileSystemWatcher fswCommonDesktop = new FileSystemWatcher(Environment.GetFolderPath(Environment.SpecialFolder.CommonDesktopDirectory));

        public MainWindow()
        {
            InitializeComponent();
            fswDesktop.Changed += FswDesktop_Changed;
            fswDesktop.Created += FswDesktop_Changed;
            fswDesktop.Deleted += FswDesktop_Changed;
            fswDesktop.Renamed += FswDesktop_Changed;

            fswCommonDesktop.Changed += FswDesktop_Changed;
            fswCommonDesktop.Created += FswDesktop_Changed;
            fswCommonDesktop.Deleted += FswDesktop_Changed;
            fswCommonDesktop.Renamed += FswDesktop_Changed;

            fswDesktop.EnableRaisingEvents = true;
            fswCommonDesktop.EnableRaisingEvents = true;
        }

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr FindWindow([MarshalAs(UnmanagedType.LPTStr)] string lpClassName, [MarshalAs(UnmanagedType.LPTStr)] string lpWindowName);
        [DllImport("user32.dll")]
        public static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);



        private void Window_StateChanged(object sender, EventArgs e)
        {
            if (WindowState != WindowState.Maximized)
                WindowState = WindowState.Maximized;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (handle == IntPtr.Zero) handle = new WindowInteropHelper(this).Handle;

            IntPtr hwndParent = FindWindow("ProgMan", null);
            IntPtr next = FindWindowEx(hwndParent, IntPtr.Zero, "SHELLDLL_DefView", null);
            SetParent(handle, next);
            desktopHandle = FindWindowEx(next, IntPtr.Zero, "SysListView32", "FolderView");
            if(desktopHandle!=IntPtr.Zero)
            {
                ShowWindow(desktopHandle, 0);
                //Убираем сущ
            }
            this.Topmost = true;
            WindowState = WindowState.Maximized;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (desktopHandle != IntPtr.Zero)
            {
                ShowWindow(desktopHandle, 1);
                //Возвращаем
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_Drop(object sender, DragEventArgs e)
        {
            //Console.WriteLine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory));
            //Console.WriteLine(Environment.GetFolderPath(Environment.SpecialFolder.CommonDesktopDirectory));
            if (e.Data.GetDataPresent(DataFormats.FileDrop) == true)
            {
                string[] FileList = (string[])e.Data.GetData(DataFormats.FileDrop, false);
                foreach (string File in FileList)
                    Console.WriteLine(File);
            }
        }

        private void FswDesktop_Changed(object sender, FileSystemEventArgs e)
        {
            Console.WriteLine(e.ChangeType);
            //throw new NotImplementedException();
        }
    }
}