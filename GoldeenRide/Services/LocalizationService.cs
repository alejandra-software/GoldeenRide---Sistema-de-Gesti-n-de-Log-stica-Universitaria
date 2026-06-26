using System.Globalization;

namespace GoldeenRide.Services;

/// <summary>

/// </summary>
public class LocalizationService
{
    private static LocalizationService? _instance;
    public static LocalizationService Instance => _instance ??= new LocalizationService();

    private LocalizationService() { }

    /// <summary>

    /// </summary>
    public string GetString(string key)
    {
        if (Application.Current?.Resources.TryGetValue(key, out var value) == true)
            return value?.ToString() ?? key;
        return key;
    }

    /// <summary>
    /// Obtiene el idioma actual del dispositivo
    /// </summary>
    public string GetCurrentLanguageCode()
    {
        return CultureInfo.CurrentUICulture.TwoLetterISOLanguageName.ToLower();
    }
}
