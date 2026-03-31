using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace StarPublications.Converters
{
    /// <summary>
    /// Returns <see cref="Visibility.Visible"/> when the bound value is not null/empty,
    /// and <see cref="Visibility.Collapsed"/> otherwise.
    /// </summary>
    [ValueConversion(typeof(string), typeof(Visibility))]
    public class StringToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return string.IsNullOrWhiteSpace(value as string)
                ? Visibility.Collapsed
                : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

    /// <summary>
    /// Returns <see cref="Visibility.Visible"/> when the boolean is true,
    /// <see cref="Visibility.Collapsed"/> when false.
    /// Pass "Invert" as the converter parameter to flip the logic.
    /// </summary>
    [ValueConversion(typeof(bool), typeof(Visibility))]
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool boolValue = value is bool b && b;
            bool invert = parameter as string == "Invert";
            bool visible = invert ? !boolValue : boolValue;
            return visible ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

    /// <summary>
    /// Returns <see cref="Visibility.Visible"/> when the bound object is not null;
    /// <see cref="Visibility.Collapsed"/> when null.
    /// Pass "Invert" as converter parameter to reverse the logic.
    /// </summary>
    [ValueConversion(typeof(object), typeof(Visibility))]
    public class NullToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isNull = value == null;
            bool invert = parameter as string == "Invert";
            bool visible = invert ? isNull : !isNull;
            return visible ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

    /// <summary>
    /// Formats a nullable decimal as a currency string, or returns "N/A" if null.
    /// </summary>
    [ValueConversion(typeof(decimal?), typeof(string))]
    public class NullableCurrencyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is decimal d)
                return d.ToString("C", culture);
            return "N/A";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

    /// <summary>
    /// Returns <see cref="Visibility.Collapsed"/> when the boolean is true (hides loading spinner when not busy).
    /// </summary>
    [ValueConversion(typeof(bool), typeof(Visibility))]
    public class InvertBoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool boolValue = value is bool b && b;
            return boolValue ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}
