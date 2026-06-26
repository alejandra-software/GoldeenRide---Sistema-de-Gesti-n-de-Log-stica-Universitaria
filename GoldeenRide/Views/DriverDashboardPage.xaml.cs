using GoldeenRide.ViewModels;

namespace GoldeenRide.Views;

public partial class DriverDashboardPage : ContentPage
{
    public DriverDashboardPage()
    {
        InitializeComponent();
        BindingContext = new DriverDashboardViewModel();
    }

   
    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is DriverDashboardViewModel vm)
        {
            vm.LoadDriverDataAsyncCommand.Execute(null);
        }
    }
}