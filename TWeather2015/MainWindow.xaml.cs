using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.IO;
using System.Windows.Controls;
using System.Windows.Input;
using TWeather2015.Theme;
using System.Collections.Generic;
using System.Threading;
using NuGetUpdate.Shared;
using Peter;

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

        private string secretKey = "W[='Mjp09!h3UCp";

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
        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("user32.dll", SetLastError = true)]
        static extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        const uint WM_RBUTTONDOWN = 0x0204;
        const uint WM_RBUTTONUP = 0x0205;



        private void Window_StateChanged(object sender, EventArgs e)
        {
            if (WindowState != WindowState.Maximized)
                WindowState = WindowState.Maximized;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (handle == IntPtr.Zero) handle = new WindowInteropHelper(this).Handle;

            IntPtr hwndParent = FindWindow("ProgMan", null);
            hwndParent2 = FindWindowEx(hwndParent, IntPtr.Zero, "SHELLDLL_DefView", null);
            SetParent(handle, hwndParent2);
            desktopHandle = FindWindowEx(hwndParent2, IntPtr.Zero, "SysListView32", "FolderView");
            if (desktopHandle != IntPtr.Zero)
            {
                ShowWindow(desktopHandle, 0);
                //Убираем сущ
            }
            this.Topmost = false;
            WindowState = WindowState.Maximized;
            dIconManager.reCreateGrid(Width, Height);
            dIconManager.reCalculateItems();
            {
                List<string> files = new List<string>();
                files.AddRange(Directory.GetDirectories(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory)));
                files.AddRange(Directory.GetDirectories(Environment.GetFolderPath(Environment.SpecialFolder.CommonDesktopDirectory)));
                files.AddRange(Directory.GetFiles(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory)));
                files.AddRange(Directory.GetFiles(Environment.GetFolderPath(Environment.SpecialFolder.CommonDesktopDirectory)));
                dIconManager.loadIcons(files.ToArray());
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
            if (!e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.None;
                e.Handled = true;
            }
            string[] FileList = (string[])e.Data.GetData(DataFormats.FileDrop, false);
            if (FileList.Length == 0) { e.Handled = true; return; }
            
            bool selfDroped = e.Data.GetDataPresent(DataFormats.UnicodeText) ? e.Data.GetData(DataFormats.Text).ToString() == secretKey : false;
            Point mouseDropPos = e.GetPosition(gridMain);
            if (e.KeyStates.HasFlag(DragDropKeyStates.AltKey) && e.AllowedEffects.HasFlag(DragDropEffects.Link))
            {
                e.Effects = DragDropEffects.Link;
                e.Handled = true;
                linkToDesktop(FileList, mouseDropPos, selfDroped);
            }
            else if (e.KeyStates.HasFlag(DragDropKeyStates.ControlKey) && e.AllowedEffects.HasFlag(DragDropEffects.Copy))
            {
                e.Effects = DragDropEffects.Copy;
                e.Handled = true;
                copyToDesktop(FileList, mouseDropPos, selfDroped);
            }
            else if (e.AllowedEffects.HasFlag(DragDropEffects.Move))
            {
                e.Effects = DragDropEffects.Move;
                e.Handled = true;
                moveToDesktop(FileList, mouseDropPos, selfDroped);
            }
            else if (e.AllowedEffects.HasFlag(DragDropEffects.Link))
            {
                e.Effects = DragDropEffects.Link;
                e.Handled = true;
                linkToDesktop(FileList, mouseDropPos, selfDroped);
            }
        }

        private void FswDesktop_Changed(object sender, FileSystemEventArgs e)
        {
            switch(e.ChangeType)
            {
                case WatcherChangeTypes.Created:
                    dIconManager.addNewIcon(e.FullPath);
                    break;
                case WatcherChangeTypes.Renamed:
                    if (e is RenamedEventArgs)
                    {
                        RenamedEventArgs e1 = e as RenamedEventArgs;
                        dIconManager.reName(e1.OldFullPath, e1.FullPath);
                    }
                    break;
                case WatcherChangeTypes.Deleted:
                    dIconManager.delete(e.FullPath);
                    break;
                case WatcherChangeTypes.Changed:
                    dIconManager.updateIcon(e.FullPath);
                    break;
            }
            Console.WriteLine(e.ChangeType);
            //throw new NotImplementedException();
        }

        private void Window_DragOver(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.None;
                e.Handled = true;
            }
            if (e.KeyStates.HasFlag(DragDropKeyStates.AltKey) && e.AllowedEffects.HasFlag(DragDropEffects.Link))
            {
                e.Effects = DragDropEffects.Link;
                e.Handled = true;
                //Cursor = System.Windows.Input.Cursors.AppStarting;
            }
            else if (e.KeyStates.HasFlag(DragDropKeyStates.ControlKey) && e.AllowedEffects.HasFlag(DragDropEffects.Copy))
            {
                e.Effects = DragDropEffects.Copy;
                e.Handled = true;
                //Cursor = System.Windows.Input.Cursors.AppStarting;
            }
            else if (e.KeyStates.HasFlag(DragDropKeyStates.ShiftKey) && e.AllowedEffects.HasFlag(DragDropEffects.Move))
            {
                e.Effects = DragDropEffects.Move;
                e.Handled = true;
                //Cursor = System.Windows.Input.Cursors.AppStarting;
            }
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
            /*if(e.ChangedButton == MouseButton.Right)
            {
                var pt = e.GetPosition(gridMain);
                //Console.WriteLine("{0:X} - {1:X} - {2:X}", (ushort)pt.X, (ushort)pt.Y << 16 , );
                IntPtr pt2 = new IntPtr(((ushort)pt.X) + ((ushort)pt.Y << 16));
                PostMessage(hwndParent2, WM_RBUTTONDOWN, IntPtr.Zero, pt2);
                PostMessage(hwndParent2, WM_RBUTTONUP, IntPtr.Zero, pt2);
                return;
                Close();
            }*/
            gridMain.ReleaseMouseCapture();
            mouseState = MouseState.None;
            Console.WriteLine(mouseState);
        }

        private void gridMain_MouseDown(object sender, MouseButtonEventArgs e)
        {
            switch (mouseState)
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
                        doDragAndDrop();
                        Console.WriteLine(mouseState);
                    }
                    break;
            }
        }

        DIcon dIconTemp = null;
        int previousClick = (int)doubleClickTime;
        [DllImport("user32.dll")]
        static extern uint GetDoubleClickTime();
        static uint doubleClickTime = GetDoubleClickTime();
        private IntPtr hwndParent2;

        internal void DIcon_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            switch (mouseState)
            {
                case MouseState.None:
                    if (e.ChangedButton == MouseButton.Left && sender is DIcon)
                    {
                        if (Environment.TickCount - previousClick < doubleClickTime)
                        {
                            //Console.WriteLine("TickCoubt {0} - {1} = {2} < {3} ", Environment.TickCount, previousClick, Environment.TickCount - previousClick, doubleClickTime);
                            previousClick = (int)doubleClickTime;
                            (sender as DIcon).start();
                        }
                        else
                        {
                            mouseDownPos = e.GetPosition(gridMain);
                            gridMain.CaptureMouse();
                            dIconTemp = sender as DIcon;
                            mouseState = MouseState.Drag;
                            Console.WriteLine(mouseState);
                            previousClick = Environment.TickCount;
                        }
                    }
                    break;
            }
        }

        internal void doDragAndDrop()
        {
            DataObject data = dIconManager.getDataForDragAndDrop();
            if (data == null) return;
            data.SetText(secretKey);
            DragDrop.DoDragDrop(this, data, DragDropEffects.Copy | DragDropEffects.Move);
            gridMain.ReleaseMouseCapture();
            mouseState = MouseState.None;
            Console.WriteLine(mouseState);
        }

        private void moveToDesktop(string[] fileList, Point pos, bool selfDroped)
        {
            if(selfDroped)
            {
                int x = (int)(pos.X / 76);
                int y = (int)(pos.Y / 100);
                dIconManager.moveTo(fileList, x, y);
            }
            //Console.WriteLine("selfDroped : {0}", selfDroped);
            //Проверяем если это местные файлы десктоп
            //Проверяем существуют ли файлы
            //Если Всё ок то переносим
            //Знаю знаю, плохо, но не хочу сюрпризов
            try
            {
                foreach (var oldfile in fileList)
                {
                    string newfile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), Path.GetFileName(oldfile));
                    if (isFolder(oldfile)) Directory.Move(oldfile, newfile);
                    else File.Move(oldfile, newfile);
                    //Console.WriteLine(newfile);
                    //File.Move();
                    dIconManager.updateIcon(newfile);
                }
            }
            catch (Exception e) { new Thread(new ThreadStart(delegate { MessageBox.Show(e.Message, "Exception", MessageBoxButton.OK, MessageBoxImage.Error); })).Start(); }
        }

        private void copyToDesktop(string[] fileList, Point pos, bool selfDroped)
        {
            if (selfDroped) return;
            //Знаю знаю, плохо, но не хочу сюрпризов
            try
            {
                foreach (var oldfile in fileList)
                {
                    string newfile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), Path.GetFileName(oldfile));
                    if (isFolder(oldfile)) { }//Directory.(oldfile, newfile);
                    else File.Copy(oldfile, newfile);
                    //Console.WriteLine(newfile);
                    //File.Move();
                    dIconManager.updateIcon(newfile);
                }
            }
            catch (Exception e) { new Thread(new ThreadStart(delegate { MessageBox.Show(e.Message, "Exception", MessageBoxButton.OK, MessageBoxImage.Error); })).Start(); }
        }

        private void linkToDesktop(string[] fileList, Point pos, bool selfDroped)
        {
            if (selfDroped) return;
            //Знаю знаю, плохо, но не хочу сюрпризов
            try
            {
                foreach (var oldfile in fileList)
                {
                    string newfile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), Path.GetFileNameWithoutExtension(oldfile)) + ".lnk";
                    using (ShellLink shortcut = new ShellLink())
                    {
                        shortcut.Target = oldfile;
                        shortcut.WorkingDirectory = Path.GetDirectoryName(oldfile);
                        shortcut.Description = Path.GetFileNameWithoutExtension(oldfile);
                        shortcut.DisplayMode = LinkDisplayMode.Normal;
                        shortcut.Save(newfile);
                    }
                    dIconManager.updateIcon(newfile);
                }
            }
            catch (Exception e) { new Thread(new ThreadStart(delegate { MessageBox.Show(e.Message, "Exception", MessageBoxButton.OK, MessageBoxImage.Error); })).Start(); }
        }

        private static bool isFolder(string path)
        {
            return path.EndsWith("\\") || Directory.Exists(path);
        }

        internal void DIcon_PreviewMouseUp(string[] v, MouseButtonEventArgs e)
        {
            ShellContextMenu scm = new ShellContextMenu();
            List<FileInfo> fis = new List<FileInfo>();
            foreach (string str in v)
            {
                fis.Add(new FileInfo(str));
            }
            var p = new System.Drawing.Point() { X = (int)e.GetPosition(gridMain).X, Y = (int)e.GetPosition(gridMain).Y };
            scm.ShowContextMenu(fis.ToArray(), p);
            e.Handled = true;
            Console.WriteLine("right Click for {0}", v.Length);
        }
    }
}