namespace ArcOrganization.Infrastructure.Converters
{
    using System;
    using System.Globalization;
    using System.Windows.Data;

    public class BytesToMegabytesTextConverter : IValueConverter 
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var bytes = (double)value;
            var megaBytes = (bytes / 1024f) / 1024f;
            return string.Format("{0} MB", megaBytes.ToString("F2"));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
