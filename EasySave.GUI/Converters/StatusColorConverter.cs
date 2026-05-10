using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using EasySave.Core.Models;

namespace EasySave.GUI.Converters
{
    public class StatusColorConverter : IValueConverter
    {
        public object Convert(
            object value,
            Type targetType,
            object parameter,
            CultureInfo culture)
        {
            if (value is JobStatus status)
            {
                return ConvertStatusToBrush(status);
            }

            string statusText = value?.ToString() ?? string.Empty;

            return statusText switch
            {
                nameof(JobStatus.Actif) => Brushes.Green,
                nameof(JobStatus.EnPause) => Brushes.Orange,
                nameof(JobStatus.Terminé) => Brushes.Gray,
                nameof(JobStatus.Erreur) => Brushes.Red,
                nameof(JobStatus.Interrompu) => Brushes.DarkRed,
                _ => Brushes.White
            };
        }

        public object ConvertBack(
            object value,
            Type targetType,
            object parameter,
            CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        private static Brush ConvertStatusToBrush(JobStatus status)
        {
            return status switch
            {
                JobStatus.Actif => Brushes.Green,
                JobStatus.EnPause => Brushes.Orange,
                JobStatus.Terminé => Brushes.Gray,
                JobStatus.Erreur => Brushes.Red,
                JobStatus.Interrompu => Brushes.DarkRed,
                _ => Brushes.White
            };
        }
    }
}