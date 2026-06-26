using GoldeenRide.Services;
using System.Globalization;
using System.Linq;

namespace GoldeenRide;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
        SetAppLanguage();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(new AppShell());
    }

    private void SetAppLanguage()
    {
        string idiomaCelular = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
        ResourceDictionary diccionarioIdioma;

        if (idiomaCelular == "es")
            diccionarioIdioma = new GoldeenRide.Resources.Translations.AppResourcesEs();
        else
            diccionarioIdioma = new GoldeenRide.Resources.Translations.AppResourcesEn();

        if (Current != null)
        {
            var diccionarioAnterior = Current.Resources.MergedDictionaries.LastOrDefault();
            if (diccionarioAnterior != null)
                Current.Resources.MergedDictionaries.Remove(diccionarioAnterior);

            Current.Resources.MergedDictionaries.Add(diccionarioIdioma);
        }
    }

    protected override async void OnStart()
    {
        base.OnStart();

        bool sesionActiva = await SupabaseService.Instance.IsSessionActiveAsync();

        if (sesionActiva)
        {
            var usuario = SupabaseService.Instance.GetCurrentUser();
            if (usuario != null && !string.IsNullOrEmpty(usuario.Id))
            {
                var datos = await SupabaseService.Instance.GetUserDataAsync(usuario.Id!); 

                if (datos != null)
                {
                    await ViewModels.AppShellViewModel.Instance.UpdateMenuStateAsync();

                    if (datos.Rol.ToLower().Contains("pasajero"))
                        await Shell.Current!.GoToAsync("///passenger-dashboard");
                    else
                        await Shell.Current!.GoToAsync("///driver-dashboard");
                }
            }
        }
    }
}