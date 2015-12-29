using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.IO;
using System.Windows.Controls;
using System.Windows.Input;
using TWeather2015.Theme;

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
        /*
        1. Сделать иконки
        2. Сделать контекстное мею
        3. Сделать сохранение изменений в файл
        4. Отрегулировать драг & дроп
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
            dIconManager.reCreateGrid(Width,Height);
            dIconManager.reCalculateItems();
            {
                foreach (var fl in Directory.GetDirectories(
                                       Environment.GetFolderPath(
                                           Environment.SpecialFolder.DesktopDirectory)))
                {
                    dIconManager.addNewIcon(fl);
                }
                foreach (var fl in Directory.GetFiles(
                                       Environment.GetFolderPath(
                                           Environment.SpecialFolder.DesktopDirectory)))
                {
                    dIconManager.addNewIcon(fl);
                }
            }
            //dIconManager.UpdateLayout();
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
                    Console.WriteLine("{0} {1}",e.AllowedEffects,File);
            }
        }

        private void FswDesktop_Changed(object sender, FileSystemEventArgs e)
        {
            Console.WriteLine(e.ChangeType);
            //throw new NotImplementedException();
        }

        private void Window_DragOver(object sender, DragEventArgs e)
        {
            e.Handled = true;
            /*if( e.KeyStates.HasFlag(DragDropKeyStates.AltKey) && e.AllowedEffects.HasFlag(DragDropEffects.Link))
            {
                Console.WriteLine("test");
                e.Effects = DragDropEffects.Link;
                e.Handled = true;
                //Cursor = System.Windows.Input.Cursors.AppStarting;
            }*/
        }
        enum MouseState
        {
            None, Rectangle, Drag, Dragging
        }

        MouseState mouseState = MouseState.None;
        Point mouseDownPos;

        private void gridMain_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                switch (mouseState)
                {
                    case MouseState.Rectangle:
                        selectionBox.Visibility = Visibility.Collapsed;
                        Point mouseUpPos = e.GetPosition(gridMain);

                        double left = Math.Min(mouseDownPos.X, mouseUpPos.X);
                        double right = Math.Max(mouseDownPos.X, mouseUpPos.X);
                        double top = Math.Min(mouseDownPos.Y, mouseUpPos.Y);
                        double bottom = Math.Max(mouseDownPos.Y, mouseUpPos.Y);
                        if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control) || Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
                            dIconManager.selectAllAdd(true, left, top, right, bottom);
                        else dIconManager.selectAll(true, left, top, right, bottom);

                        break;
                    case MouseState.Drag:
                        if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control) || Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
                        { if (dIconTemp != null) dIconManager.selectAdd(dIconTemp); }
                        else if (dIconTemp != null) dIconManager.select(dIconTemp);

                        break;
                    case MouseState.Dragging:
                        break;
                }
            }
            gridMain.ReleaseMouseCapture();
            mouseState = MouseState.None;
            Console.WriteLine(mouseState);
        }

        private void gridMain_MouseDown(object sender, MouseButtonEventArgs e)
        {
            switch(mouseState)
            {
                case MouseState.None:
                    if (e.ChangedButton == MouseButton.Left)
                    {
                        mouseDownPos = e.GetPosition(gridMain);
                        gridMain.CaptureMouse();
                        Canvas.SetLeft(selectionBox, mouseDownPos.X);
                        Canvas.SetTop(selectionBox, mouseDownPos.Y);
                        selectionBox.Width = 0;
                        selectionBox.Height = 0;
                        selectionBox.Visibility = Visibility.Visible;
                        mouseState = MouseState.Rectangle;
                        Console.WriteLine(mouseState);
                    }
                    break;
            }
        }

        private void gridMain_MouseMove(object sender, MouseEventArgs e)
        {
            switch (mouseState)
            {
                case MouseState.Rectangle:
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
                    break;
                case MouseState.Drag:
                    Point current = e.GetPosition(gridMain);
                    if (Math.Abs(current.X - mouseDownPos.X) >= SystemParameters.MinimumHorizontalDragDistance ||
                        Math.Abs(current.Y - mouseDownPos.Y) >= SystemParameters.MinimumVerticalDragDistance)
                    {
                        if (dIconTemp != null) dIconManager.prepareForDragAndDrop(dIconTemp);
                        mouseState = MouseState.Dragging;
                        Console.WriteLine(mouseState);
                    }
                    break;
            }
        }

        DIcon dIconTemp = null;

        internal void DIcon_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            switch(mouseState)
            {
                case MouseState.None:
                    if(e.ChangedButton == MouseButton.Left && sender is DIcon)
                    {
                        
                        mouseDownPos = e.GetPosition(gridMain);
                        gridMain.CaptureMouse();
                        dIconTemp = sender as DIcon;
                        mouseState = MouseState.Drag;
                        Console.WriteLine(mouseState);
                    }
                    break;
            }
        }
    }
}