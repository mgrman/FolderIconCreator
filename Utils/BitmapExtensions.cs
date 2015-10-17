using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace FolderIconCreator
{
    public static class BitmapExtensions
    {
        public static BitmapSource ToBitmapSource(this System.Drawing.Bitmap bmp)
        {
            return System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
               bmp.GetHbitmap(),
               IntPtr.Zero,
               System.Windows.Int32Rect.Empty,
               BitmapSizeOptions.FromWidthAndHeight(bmp.Width, bmp.Height));
        }
    }
}
