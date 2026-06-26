using GoldeenRide.ViewModels;

namespace GoldeenRide.Views;

public partial class LoginPage : ContentPage
{
    public LoginPage()
    {
        InitializeComponent();
        BindingContext = AuthViewModel.Instance;
    }
}