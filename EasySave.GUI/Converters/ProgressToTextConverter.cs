using System;
using System.Globalization;
using System.Windows.Data;

namespace EasySave.GUI.Converters
{
    public class ProgressToTextConverter : IValueConverter
    {
        public object Convert(
            object value,
            Type targetType,
            object parameter,
            CultureInfo culture)
        {
            if (value is double progression)
            {
                return $"{progression:0.0}%";
            }

            if (value is int integerProgression)
            {
                return $"{integerProgression}%";
            }

            return "0%";
        }

        public object ConvertBack(
            object value,
            Type targetType,
            object parameter,
            CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}