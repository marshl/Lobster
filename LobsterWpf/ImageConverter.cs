using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace LobsterWpf
{
    class ImageConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var image = values[0] as BitmapImage;

            string s = values[1].ToString();

            bool isLogged = System.Convert.ToBoolean(s);

            if (!isLogged)
            {
                try
                {
                    if (image != null)
                    {
                        var grayBitmapSource = new FormatConvertedBitmap();
                        grayBitmapSource.BeginInit();
                        grayBitmapSource.Source = image;
                        grayBitmapSource.DestinationFormat = PixelFormats.Gray32Float;
                        grayBitmapSource.EndInit();
                        return grayBitmapSource;
                    }
                    return null;

                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            return image;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
