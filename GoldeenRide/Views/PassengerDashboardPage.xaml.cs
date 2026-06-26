using GoldeenRide.ViewModels;

namespace GoldeenRide.Views;

public partial class PassengerDashboardPage : ContentPage
{
    public PassengerDashboardPage()
    {
        InitializeComponent();
        BindingContext = new PassengerDashboardViewModel();
    }
}