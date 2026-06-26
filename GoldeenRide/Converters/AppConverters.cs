using System.Globalization;

namespace GoldeenRide.Converters;

/// <summary>

/// </summary>
public class InvertedBoolConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool b)
            return !b;
        return false;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool b)
            return !b;
        return false;
    }
}

/// <summary>
/// Convierte un texto en bool: true si tiene contenido, false si está vacío
/// Útil para mostrar/ocultar mensajes de error solo cuando existen
/// </summary>
public class StringToBoolConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string s)
            return !string.IsNullOrWhiteSpace(s);
        return false;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Convierte el estado "contraseña visible" en el emoji del ojito correspondiente
/// true (visible) → 🙈 (ocultar)
/// false (oculta) → 👁️ (mostrar)
/// </summary>
public class PasswordVisibilityIconConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isVisible)
            return isVisible ? "🙈" : "👁️";
        return "👁️";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
