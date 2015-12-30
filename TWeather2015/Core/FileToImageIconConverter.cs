using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using vbAccelerator.Components.ImageList;

namespace TWeather2015.Core
{
    public class FileToImageIconConverter
    {
        private static SysImageList _imgList = new SysImageList(SysImageListSize.jumbo);

        private string filePath;
        private System.Windows.Media.ImageSource icon;

        public string FilePath { get { return filePath; } }

        public System.Windows.Media.ImageSource Image
        {
            get
            {
                if (icon == null && (System.IO.File.Exists(FilePath) || Directory.Exists(FilePath)))
                {
                    _imgList.ImageListSize = SysImageListSize.extraLargeIcons;
                    using (Icon sysicon = _imgList.Icon(_imgList.IconIndex(FilePath, isFolder(FilePath))))
                    {
                        icon = System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(
                                  sysicon.Handle,
                                  System.Windows.Int32Rect.Empty,
                                  System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
                    }
                }

                return icon;
            }
        }

        private static bool isFolder(string path)
        {
            return path.EndsWith("\\") || Directory.Exists(path);
        }

        public FileToImageIconConverter(string filePath)
        {
            this.filePath = filePath;
        }
    }
}
