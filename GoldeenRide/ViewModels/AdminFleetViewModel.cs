using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GoldeenRide.Models;
using GoldeenRide.Services;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using System;

namespace GoldeenRide.ViewModels;

public partial class AdminFleetViewModel : ObservableObject
{
    private readonly SupabaseService _supabaseService = SupabaseService.Instance;

    public ObservableCollection<Vehiculo> Vehiculos { get; } = new();
    public ObservableCollection<Usuario> Empleados { get; } = new();

    [ObservableProperty] private bool isLoading = false;

    public AdminFleetViewModel()
    {
        _ = LoadDataAsync();
    }

    [RelayCommand]
    public async Task LoadDataAsync()
    {
        IsLoading = true;
        var user = _supabaseService.GetCurrentUser();
        if (user != null && !string.IsNullOrEmpty(user.Id))
        {
            var vehiculosDb = await _supabaseService.GetVehiclesByOwnerAsync(user.Id);
            Vehiculos.Clear();
            foreach (var v in vehiculosDb) Vehiculos.Add(v);

            var empleadosDb = await _supabaseService.GetEmployeesByBossAsync(user.Id);
            Empleados.Clear();
            foreach (var e in empleadosDb) Empleados.Add(e);
        }
        IsLoading = false;
    }

    [RelayCommand]
    public async Task AddVehicleAsync()
    {
        if (Shell.Current == null) return;

        string placa = await Shell.Current.DisplayPromptAsync("Nuevo Microbús", "Ingresa la Placa (Ej: MB-1234):", "Guardar", "Cancelar");
        if (string.IsNullOrWhiteSpace(placa)) return;

        string capacidadStr = await Shell.Current.DisplayPromptAsync("Capacidad", "¿Cuántos pasajeros caben?", "Guardar", "Cancelar", "35", maxLength: 2, keyboard: Keyboard.Numeric);
        int capacidad = int.TryParse(capacidadStr, out int c) ? c : 35;

        var user = _supabaseService.GetCurrentUser();
        if (user == null || string.IsNullOrEmpty(user.Id)) return;

        var nuevoVehiculo = new Vehiculo
        {
            IdPropietario = user.Id,
            Placa = placa.ToUpper(),
            Capacidad = capacidad,
            CreadoEn = DateTime.UtcNow
        };

        bool success = await _supabaseService.AddVehicleAsync(nuevoVehiculo);
        if (success)
        {
            Vehiculos.Add(nuevoVehiculo);
            await Shell.Current.DisplayAlertAsync("Éxito", "Vehículo agregado a tu flota.", "OK");
        }
        else
        {
            await Shell.Current.DisplayAlertAsync("Error", "No se pudo guardar el vehículo.", "OK");
        }
    }

    [RelayCommand]
    public async Task LinkEmployeeAsync()
    {
        if (Shell.Current == null) return;

        string email = await Shell.Current.DisplayPromptAsync("Vincular Empleado", "Ingresa el correo electrónico del Chofer Empleado:", "Vincular", "Cancelar", keyboard: Keyboard.Email);

        if (string.IsNullOrWhiteSpace(email)) return;

        var user = _supabaseService.GetCurrentUser();
        if (user == null || string.IsNullOrEmpty(user.Id)) return;

        IsLoading = true;
        var (success, message) = await _supabaseService.LinkEmployeeByEmailAsync(email.Trim().ToLower(), user.Id);

        if (success)
        {
            await Shell.Current.DisplayAlertAsync("Éxito", message, "OK");
            await LoadDataAsync();
        }
        else
        {
            await Shell.Current.DisplayAlertAsync("Atención", message, "OK");
        }
        IsLoading = false;
    }
}