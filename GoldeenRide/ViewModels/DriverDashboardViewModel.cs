using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GoldeenRide.Models;
using GoldeenRide.Services;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace GoldeenRide.ViewModels;

public partial class DriverDashboardViewModel : ObservableObject
{
    private readonly SupabaseService _supabaseService = SupabaseService.Instance;

    [ObservableProperty] private string driverName = "Cargando...";
    [ObservableProperty] private string assignedVehicle = "Buscando...";

    [ObservableProperty] private string nextTripType = "Sin viajes programados";
    [ObservableProperty] private string nextTripTime = "--:--";
    [ObservableProperty] private string nextTripRoute = "N/A";
    [ObservableProperty] private string reservedSeats = "0 / 35";
    [ObservableProperty] private double occupancyProgress = 0.0;

    [ObservableProperty] private bool hasNextTrip = false;

    [ObservableProperty] private bool isFleetManager = false;
    [ObservableProperty] private bool isIndependentDriver = false;
    [ObservableProperty] private bool isEmployee = false;
    [ObservableProperty] private bool canScheduleTrips = false;

    private ObservableCollection<Viaje> MisViajes { get; } = new();

    public DriverDashboardViewModel()
    {
        // La carga ahora en OnAppearing de la vista
    }

    [RelayCommand]
    public async Task LoadDriverDataAsync()
    {
        //  Apagar los roles antes de verificar
        DriverName = "Cargando...";
        IsFleetManager = false;
        IsIndependentDriver = false;
        IsEmployee = false;
        CanScheduleTrips = false;
        HasNextTrip = false;
        MisViajes.Clear();

        try
        {
            var currentUser = _supabaseService.GetCurrentUser();
            if (currentUser == null || string.IsNullOrEmpty(currentUser.Id)) return;

            var userData = await _supabaseService.GetUserDataAsync(currentUser.Id!);
            if (userData == null) return;

            DriverName = userData.Nombre;
            string rolLower = userData.Rol.ToLower();

            // Asignación limpia y estricta
            IsFleetManager = rolLower.Contains("jefe") || rolLower.Contains("owner");
            IsIndependentDriver = rolLower.Contains("independiente") || rolLower.Contains("independent");
            IsEmployee = rolLower.Contains("empleado");

            CanScheduleTrips = IsFleetManager || IsIndependentDriver;

            var todosViajes = await _supabaseService.GetAllActiveTripsAsync();

            if (IsFleetManager)
            {
                var empleados = await _supabaseService.GetEmployeesByBossAsync(currentUser.Id!);
                var idsFlota = empleados.Select(e => e.Id).ToList();
                idsFlota.Add(currentUser.Id!);

                foreach (var v in todosViajes.Where(v => idsFlota.Contains(v.IdChofer)))
                    MisViajes.Add(v);
            }
            else
            {
                foreach (var v in todosViajes.Where(v => v.IdChofer == currentUser.Id))
                    MisViajes.Add(v);
            }

            var proximoViaje = MisViajes.OrderBy(v => v.HoraSalida).FirstOrDefault(v => v.HoraSalida >= DateTime.UtcNow);

            if (proximoViaje != null)
            {
                HasNextTrip = true;
                NextTripType = proximoViaje.TipoViaje;
                NextTripTime = proximoViaje.HoraSalida.ToLocalTime().ToString("hh:mm tt");
                NextTripRoute = proximoViaje.RutaGeneral;
                AssignedVehicle = "Microbús Asignado";
            }
            else
            {
                HasNextTrip = false;
                AssignedVehicle = "Esperando asignación...";
            }

            _ = AppShellViewModel.Instance.UpdateMenuStateAsync();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error cargando dashboard: {ex.Message}");
        }
    }

    [RelayCommand]
    public async Task OpenBoardingList()
    {
        if (Shell.Current != null) await Shell.Current.DisplayAlert("Lista", "Aquí cargará la lista de estudiantes.", "OK");
    }

    [RelayCommand]
    public async Task OpenAdminFleet()
    {
        if (Shell.Current != null) await Shell.Current.GoToAsync("admin-fleet");
    }

    [RelayCommand]
    public async Task OpenScheduleTrip()
    {
        if (Shell.Current != null) await Shell.Current.GoToAsync("schedule-trip");
    }

    [RelayCommand]
    public void OpenSidebar()
    {
        if (Shell.Current != null) Shell.Current.FlyoutIsPresented = true;
    }
}