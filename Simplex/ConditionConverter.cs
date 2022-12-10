using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace Labs.Simplex;

public class ConditionConverter : MarkupExtension, IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return (Condition)value switch
        {
            Condition.LessOrEqual => "<=",
            Condition.Equal => "=",
            Condition.MoreOrEqual => ">=",
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }

    private static ConditionConverter _conditionConverter = new ConditionConverter();

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        return _conditionConverter;
    }
}
