namespace ArcOrganization.Infrastructure.Converters
{
    using System;
    using System.Globalization;
    using System.Windows.Data;

    public class DateTimeYearDateTimeTextConverter : IValueConverter 
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var datetime = (DateTime)value;
            return datetime.ToString("dd.MM.yyyy");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
