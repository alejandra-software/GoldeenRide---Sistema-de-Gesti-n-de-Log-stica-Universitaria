using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GoldeenRide.Models;
using GoldeenRide.Services;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;

namespace GoldeenRide.ViewModels;

public partial class ScheduleTripViewModel : ObservableObject
{
    private readonly SupabaseService _supabaseService = SupabaseService.Instance;

    public ObservableCollection<string> TiposViaje { get; } = new() { "Ida (Hacia Universidad)", "Regreso (Hacia Casa)" };
    public ObservableCollection<Vehiculo> VehiculosDisponibles { get; } = new();
    public ObservableCollection<Usuario> ChoferesDisponibles { get; } = new();

    [ObservableProperty] private string selectedTipoViaje = string.Empty;
    [ObservableProperty] private TimeSpan horaSalida = new TimeSpan(6, 30, 0);
    [ObservableProperty] private string rutaTexto = string.Empty;
    [ObservableProperty] private Vehiculo? selectedVehiculo;
    [ObservableProperty] private Usuario? selectedChofer;

    // Variables para los días de la semana 
    [ObservableProperty] private bool diaL = true;
    [ObservableProperty] private bool diaM = true;
    [ObservableProperty] private bool diaMi = true;
    [ObservableProperty] private bool diaJ = true;
    [ObservableProperty] private bool diaV = true;
    [ObservableProperty] private bool diaS = false;
    [ObservableProperty] private bool diaD = false;

    [ObservableProperty] private bool isFleetManager = false;
    [ObservableProperty] private bool isLoading = false;

    private Usuario? _jefeActual;

    public ScheduleTripViewModel()
    {
        _ = LoadDataAsync();
    }

    [RelayCommand]
    public async Task LoadDataAsync()
    {
        IsLoading = true;
        var authUser = _supabaseService.GetCurrentUser();
        if (authUser != null && !string.IsNullOrEmpty(authUser.Id))
        {
            _jefeActual = await _supabaseService.GetUserDataAsync(authUser.Id);

            if (_jefeActual != null)
            {
                IsFleetManager = _jefeActual.Rol.ToLower().Contains("jefe");

                var vehiculos = await _supabaseService.GetVehiclesByOwnerAsync(_jefeActual.Id);
                VehiculosDisponibles.Clear();
                foreach (var v in vehiculos) VehiculosDisponibles.Add(v);

                ChoferesDisponibles.Clear();
                ChoferesDisponibles.Add(_jefeActual);

                if (IsFleetManager)
                {
                    var empleados = await _supabaseService.GetEmployeesByBossAsync(_jefeActual.Id);
                    foreach (var emp in empleados) ChoferesDisponibles.Add(emp);
                }

                if (VehiculosDisponibles.Count > 0) SelectedVehiculo = VehiculosDisponibles[0];
                SelectedChofer = _jefeActual;
            }
        }
        IsLoading = false;
    }

    [RelayCommand]
    public async Task SaveTripAsync()
    {
        if (Shell.Current == null) return;

        if (string.IsNullOrEmpty(SelectedTipoViaje) || string.IsNullOrEmpty(RutaTexto) || SelectedVehiculo == null || SelectedChofer == null)
        {
            await Shell.Current.DisplayAlertAsync("Atención", "Por favor, llena todos los campos de ruta y vehículo.", "OK");
            return;
        }

        // Armar el string de los días seleccionados
        List<string> diasSeleccionados = new();
        if (DiaL) diasSeleccionados.Add("Lu");
        if (DiaM) diasSeleccionados.Add("Ma");
        if (DiaMi) diasSeleccionados.Add("Mi");
        if (DiaJ) diasSeleccionados.Add("Ju");
        if (DiaV) diasSeleccionados.Add("Vi");
        if (DiaS) diasSeleccionados.Add("Sa");
        if (DiaD) diasSeleccionados.Add("Do");

        if (diasSeleccionados.Count == 0)
        {
            await Shell.Current.DisplayAlertAsync("Atención", "Debes seleccionar al menos un día de la semana para este viaje.", "OK");
            return;
        }

        string diasFinales = string.Join(", ", diasSeleccionados);

        IsLoading = true;

        DateTime horaCompleta = DateTime.Today.Add(HoraSalida);
        if (horaCompleta < DateTime.Now) horaCompleta = horaCompleta.AddDays(1);

        var nuevoViaje = new Viaje
        {
            IdChofer = SelectedChofer.Id,
            IdVehiculo = SelectedVehiculo.Id,
            TipoViaje = SelectedTipoViaje,
            RutaGeneral = RutaTexto,
            HoraSalida = horaCompleta.ToUniversalTime(),
            Estado = "programado",
            DiasSemana = diasFinales, // Guardamos los días
            CreadoEn = DateTime.UtcNow
        };

        bool success = await _supabaseService.CreateTripAsync(nuevoViaje);

        if (success)
        {
            await Shell.Current.DisplayAlertAsync("¡Éxito!", "El viaje recurrente ha sido publicado.", "OK");
            await Shell.Current.GoToAsync("..");
        }
        else
        {
            await Shell.Current.DisplayAlertAsync("Error", "Hubo un problema al crear el viaje.", "OK");
        }

        IsLoading = false;
    }
}