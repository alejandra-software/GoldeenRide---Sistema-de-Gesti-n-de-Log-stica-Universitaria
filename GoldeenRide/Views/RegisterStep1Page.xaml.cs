using GoldeenRide.ViewModels;

namespace GoldeenRide.Views;

public partial class RegisterStep1Page : ContentPage
{
    public RegisterStep1Page()
    {
        InitializeComponent();
        BindingContext = AuthViewModel.Instance;
    }
}