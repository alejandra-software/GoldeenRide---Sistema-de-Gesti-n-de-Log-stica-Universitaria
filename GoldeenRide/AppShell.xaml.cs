using GoldeenRide.ViewModels;

namespace GoldeenRide;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        BindingContext = AppShellViewModel.Instance;

        Routing.RegisterRoute("settings", typeof(Views.SettingsPage));
        Routing.RegisterRoute("admin-fleet", typeof(Views.AdminFleetPage));
        Routing.RegisterRoute("schedule-trip", typeof(Views.ScheduleTripPage));
    }
}