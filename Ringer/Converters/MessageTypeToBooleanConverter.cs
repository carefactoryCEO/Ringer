using System;
using System.Globalization;
using Ringer.Types;
using Xamarin.Forms;

namespace RingerStaff.Converters
{
    public class StringToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !string.IsNullOrWhiteSpace((string)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class BooleanToAgreeColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? Color.FromHex("#0082FF") : Color.LightGray;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class BooleanToOpacityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? 0.0d : 1.0d;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class InverseBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !(bool)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class MessageTypesToThicknessConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var types = (MessageTypes)value;

            if (types.HasFlag(MessageTypes.Outgoing))
                return types.HasFlag(MessageTypes.Trailing) ? new Thickness(25, 0, 15, 10) : new Thickness(25, 0, 15, 4);

            if (types.HasFlag(MessageTypes.Incomming))
                return types.HasFlag(MessageTypes.Trailing) ? new Thickness(5, 0, 25, 10) : new Thickness(5, 0, 25, 4);

            return new Thickness();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class MessageTypesToGridLengthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var types = (MessageTypes)value;
            var col = (string)parameter;

            if (types.HasFlag(MessageTypes.Incomming))
            {
                if (col == "FirstColumn")
                    return new GridLength(45, GridUnitType.Absolute);

                if (col == "ThirdColumn")
                    return new GridLength(70, GridUnitType.Absolute);
            }

            if (types.HasFlag(MessageTypes.Outgoing))
            {
                if (col == "FirstColumn")
                    return new GridLength(70, GridUnitType.Absolute);

                if (col == "ThirdColumn")
                    return new GridLength(0, GridUnitType.Absolute);
            }

            return new GridLength(1, GridUnitType.Star);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class MessageTypesToIntegerConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var types = (MessageTypes)value;

            if (types.HasFlag(MessageTypes.Incomming))
                return 2;

            if (types.HasFlag(MessageTypes.Outgoing))
                return 0;

            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class MessageTypesToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var types = (MessageTypes)value;
            string param = (string)parameter;

            if (param == "Avatar" || param == "Sender")
                return types.HasFlag(MessageTypes.Leading | MessageTypes.Incomming);

            if (param == "Timestamp")
                return types.HasFlag(MessageTypes.Trailing);

            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class MessageTypesToLayoutOptionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var types = (MessageTypes)value;
            var target = (string)parameter;

            if (target == "MainGrid")
            {
                if (types.HasFlag(MessageTypes.Incomming))
                    return new LayoutOptions(LayoutAlignment.Start, true);

                if (types.HasFlag(MessageTypes.Outgoing))
                    return new LayoutOptions(LayoutAlignment.End, true);
            }

            if (target == "Metadata")
            {
                if (types.HasFlag(MessageTypes.Incomming))
                    return new LayoutOptions(LayoutAlignment.Start, false);

                if (types.HasFlag(MessageTypes.Outgoing))
                    return new LayoutOptions(LayoutAlignment.End, false);
            }

            return new LayoutOptions(LayoutAlignment.Start, false);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class IntegerToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int count = 0;

            if (value is string)
                count = int.Parse((string)value);

            if (value is int)
                count = (int)value;

            if (value is double)
                count = (int)value;

            return count > 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class MessageTypesToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var types = (MessageTypes)value;

            if (types.HasFlag(MessageTypes.Incomming))
                return Color.FromHex("#FFFFFF");

            if (types.HasFlag(MessageTypes.Outgoing))
                return Color.FromHex("FFEB03");

            return Color.White;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class DateTimeToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var dateTime = (DateTime)value;

            return dateTime.ToLocalTime().ToString("tt h:mm");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
