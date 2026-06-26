using GoldeenRide.ViewModels;

namespace GoldeenRide.Views;

public partial class RegisterStep2Page : ContentPage
{
    public RegisterStep2Page()
    {
        InitializeComponent();
        BindingContext = AuthViewModel.Instance;
    }
}