namespace ArcOrganization.Infrastructure.Converters
{
    using System;
    using System.Net;
    using System.Globalization;
    using System.Text.RegularExpressions;
    using System.Windows.Data;

    public class HtmlTextTrimContinousConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return null;
            }

            var maxLength = 200;

            if (parameter != null)
            {
                maxLength = int.Parse(parameter.ToString());
            }

            var strLength = 0;
            var fixedString = string.Empty;

            // Remove HTML tags and newline characters from the text, and decodes HTML encoded characters. 
            // This is a basic method. Additional code would be needed to more thoroughly  
            // remove certain elements, such as embedded Javascript. 

            // Remove HTML tags. 
            fixedString = Regex.Replace(value.ToString(), "<[^>]+>", string.Empty);

            // Remove newline characters
            //fixedString = fixedString.Replace("\r", string.Empty).Replace("\n", string.Empty);

            // Remove encoded HTML characters
            fixedString = HttpUtility.HtmlDecode(fixedString);

            strLength = fixedString.Length;

            // Some feed management tools include an image tag in the Description field of an RSS feed, 
            // so even if the Description field (and thus, the Summary property) is not populated, it could still contain HTML. 
            // Due to this, after we strip tags from the string, we should return null if there is nothing left in the resulting string. 
            if (strLength == 0)
            {
                return null;
            }

            // Truncate the text if it is too long. 
            if (strLength >= maxLength)
            {
                fixedString = fixedString.Substring(0, maxLength);

                // Unless we take the next step, the string truncation could occur in the middle of a word.
                // Using LastIndexOf we can find the last space character in the string and truncate there. 
                fixedString = fixedString.Substring(0, fixedString.LastIndexOf(" "));
                fixedString += "...";
            }

            return fixedString;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
