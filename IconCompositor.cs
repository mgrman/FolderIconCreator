using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace FolderIconCreator
{
    public class IconCompositor : INotifyPropertyChanged
    {

        private static BitmapSource _folderIcon;
        protected static BitmapSource FolderIcon
        {
            get
            {
                if (_folderIcon == null)
                {
                    var iconUtil = new TsudaKageyu.IconExtractor(@"C:\Windows\system32\shell32.dll");
                    var icon = iconUtil.GetIcon(3);

                    System.Drawing.Icon[] splitIcons = TsudaKageyu.IconUtil.Split(icon);

                    var maxSize = splitIcons.Max(o => o.Width);

                    var maxIcon = splitIcons.Where(o => o.Width == maxSize).FirstOrDefault();

                    _folderIcon = Tabbles.c_icon_of_path.bitmap_source_of_icon(maxIcon);
                }
                return _folderIcon;
            }
        }


        private BitmapSource _backgroundBitmap;
        public BitmapSource BackgroundBitmap
        {
            get
            {
                return _backgroundBitmap;
            }
            set
            {
                if (_backgroundBitmap == value) return;

                _backgroundBitmap = value;
                FirePropertyChanged();

                UpdateCompositeImage();
            }
        }

        private BitmapSource _foregroundBitmap;
        public BitmapSource ForegroundBitmap
        {
            get
            {
                return _foregroundBitmap;
            }
            set
            {
                if (_foregroundBitmap == value) return;

                _foregroundBitmap = value;
                FirePropertyChanged();

                UpdateCompositeImage();
            }
        }

        private BitmapSource _compositeBitmap;
        public BitmapSource CompositeBitmap
        {
            get
            {
                return _compositeBitmap;
            }
            protected set
            {
                if (_compositeBitmap == value) return;

                _compositeBitmap = value;
                FirePropertyChanged();
            }
        }

        private double _foregroundRatio;
        public double ForegroundRatio
        {
            get
            {
                return _foregroundRatio;
            }
            set
            {
                if (_foregroundRatio == value) return;

                _foregroundRatio = value;
                UpdateCompositeImage();
            }
        }


        public IconCompositor()
        {
            BackgroundBitmap = FolderIcon;
            ForegroundRatio = 0.7;
        }



        public event PropertyChangedEventHandler PropertyChanged;

        protected void FirePropertyChanged([CallerMemberName]string propName = null)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propName));
            }
        }


        public ICommand LoadBackgroundCommand
        {
            get
            {

                return new RelayCommand<object>(o =>
                {
                    BackgroundBitmap = LoadImageOrIconUsingOFD();
                });
            }
        }

        public ICommand LoadForegroundCommand
        {
            get
            {
                return new RelayCommand<object>(o =>
                {
                    ForegroundBitmap = LoadImageOrIconUsingOFD();
                });
            }
        }


        private BitmapSource LoadImageOrIconUsingOFD()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();

            if (openFileDialog.ShowDialog() == true)
            {
                var path = openFileDialog.FileName;

                try
                {
                    return LoadImageOrIconFromPath(path);
                }
                catch
                {
                    return null;
                }
            }

            return null;
        }

        private BitmapSource LoadImageOrIconFromPath(string path)
        {
            BitmapSource bitmap = null;

            //try Image
            try
            {
                bitmap = new BitmapImage(new Uri(path));
            }
            catch
            {
                bitmap = null;
            }


            //try Dll
            if (bitmap == null && System.IO.Path.GetExtension(path) == ".dll")
            {
                //TODO
                //show dialog to select icon from dll
            }

            //other
            if (bitmap == null)
            {

                bitmap = Tabbles.c_icon_of_path.icon_of_path_large(path, true, false);
            }


            return bitmap;
        }


        public ICommand SaveCompositeCommand
        {
            get
            {
                return new RelayCommand<object>(o =>
                {
                    SaveFileDialog saveFileDialog = new SaveFileDialog();
                    saveFileDialog.Filter = "Png Image|*.png";

                    if (saveFileDialog.ShowDialog() == true)
                    {
                        var path = saveFileDialog.FileName;

                        using (var fileStream = new FileStream(path, FileMode.Create))
                        {
                            BitmapEncoder encoder = new PngBitmapEncoder();
                            encoder.Frames.Add(BitmapFrame.Create(CompositeBitmap));
                            encoder.Save(fileStream);
                        }
                    }

                },
                o => CompositeBitmap != null);
            }
        }


        protected void UpdateCompositeImage()
        {
            if (BackgroundBitmap == null || ForegroundBitmap == null || ForegroundRatio < 0 || ForegroundRatio > 1)
            {
                CompositeBitmap = null;
                return;
            }

            var resultHugeIcon = OverlayIcon(BackgroundBitmap, ForegroundBitmap, ForegroundRatio);

            CompositeBitmap = resultHugeIcon;
        }

        protected BitmapSource OverlayIcon(BitmapSource background, BitmapSource foreground, double relSize)
        {
            var drawingVisual = new DrawingVisual();
            var drawingContext = drawingVisual.RenderOpen();
            drawingContext.DrawImage(background, new Rect(new Size(background.Width, background.Height)));

            var baseRect = new Rect(background.Width - foreground.Width, background.Height - foreground.Height, foreground.Width, foreground.Height);

            var sizedRect = new Rect(baseRect.X + baseRect.Width * (1 - relSize), baseRect.Y + baseRect.Height * (1 - relSize), baseRect.Width * (relSize), baseRect.Height * (relSize));

            drawingContext.DrawImage(foreground, sizedRect);
            drawingContext.Close();

            var mergedImage = new RenderTargetBitmap((int)background.Width, (int)background.Height, background.DpiX, background.DpiY, PixelFormats.Pbgra32);
            mergedImage.Render(drawingVisual);

            return mergedImage;
        }
    }


}
