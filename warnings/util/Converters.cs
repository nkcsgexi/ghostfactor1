using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace warnings.util
{

    /* Convert a BitMap to BitmapSource. */
    public class Bitmap2SourceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
        object parameter, CultureInfo culture)
        {
            // Converts a GDI bitmap to an image source
            Bitmap bmp = value as Bitmap;
            if (bmp != null)
            {
                BitmapSource bitmapSource =
                Imaging.CreateBitmapSourceFromHBitmap(
                bmp.GetHbitmap(),
                IntPtr.Zero,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());
                return bitmapSource;
            }
            return null;
        }
        public object ConvertBack(object value,
        Type targetType, object parameter,
        System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
   
}
