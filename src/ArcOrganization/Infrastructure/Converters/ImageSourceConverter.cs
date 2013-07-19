namespace ArcOrganization.Infrastructure.Converters
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Data;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;

    public class ImageSourceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                var tn = new BitmapImage();
                tn.SetSource(
                        Application.GetResourceStream(new Uri(@"Assets/appbar.flag.bear.png", UriKind.Relative)).Stream);
                return tn;
            }

            if (value is ImageSource)
            {
                return value;
            }
           
            if (value is string)
            {
                if (string.IsNullOrEmpty(value.ToString()))
                {
                    var tn = new BitmapImage();
                    tn.SetSource(
                        Application.GetResourceStream(new Uri(@"Assets/appbar.flag.bear.png", UriKind.Relative)).Stream);
                    return tn;
                }
                else
                {
                    return new BitmapImage(new Uri(value.ToString(), UriKind.RelativeOrAbsolute));
                }
            }

            if (value is Uri)
            {
                // TODO Why this is not showing in image?
                Debug.WriteLine(value.ToString());
                return new BitmapImage(new Uri(value.ToString(), UriKind.Absolute));
            }

            throw new NotSupportedException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}