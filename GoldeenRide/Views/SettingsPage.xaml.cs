using GoldeenRide.ViewModels;

namespace GoldeenRide.Views;

public partial class SettingsPage : ContentPage
{
    public SettingsPage()
    {
        InitializeComponent();
    }

    
    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is SettingsViewModel vm)
        {
            vm.LoadProfileAsyncCommand.Execute(null);
        }
    }
}