using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GoldeenRide.Models;
using GoldeenRide.Services;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using System;
using System.Linq;

namespace GoldeenRide.ViewModels;

public partial class PassengerDashboardViewModel : ObservableObject
{
    private readonly SupabaseService _supabaseService = SupabaseService.Instance;

    // Lista de viajes disponibles que verá el estudiante
    public ObservableCollection<Viaje> ViajesDisponibles { get; } = new();

    [ObservableProperty] private string studentName = "Estudiante";
    [ObservableProperty] private bool isLoading = false;
    [ObservableProperty] private DateTime fechaSeleccionada = DateTime.Today;

    public PassengerDashboardViewModel()
    {
        _ = LoadDashboardAsync();
    }

    [RelayCommand]
    public async Task LoadDashboardAsync()
    {
        IsLoading = true;

        var authUser = _supabaseService.GetCurrentUser();
        if (authUser != null && !string.IsNullOrEmpty(authUser.Id))
        {
            var userData = await _supabaseService.GetUserDataAsync(authUser.Id);
            if (userData != null)
            {
                StudentName = userData.Nombre;
            }
        }

        await CargarViajesPorFecha(FechaSeleccionada);

        IsLoading = false;
    }

    // Este comando se ejecuta cuando el estudiante elige un día en el calendario
    [RelayCommand]
    public async Task DiaSeleccionadoCambioAsync(DateTime nuevaFecha)
    {
        FechaSeleccionada = nuevaFecha;
        await CargarViajesPorFecha(nuevaFecha);
    }

    private async Task CargarViajesPorFecha(DateTime fecha)
    {
        // Pedir todos los viajes activos a Supabase
        var todosLosViajes = await _supabaseService.GetAllActiveTripsAsync();

        ViajesDisponibles.Clear();

        //  Filtrar Mostrar solo los que salen ese día o que son recurrentes (Lunes a Viernes)
        string diaAbreviado = ObtenerAbreviaturaDia(fecha);

        var viajesDelDia = todosLosViajes.Where(v =>
            v.HoraSalida.Date == fecha.Date || // Si es un viaje único para esa fecha exacta
            v.DiasSemana.Contains(diaAbreviado) // O si es un viaje recurrente que pasa ese día
        ).OrderBy(v => v.HoraSalida.TimeOfDay).ToList();

        foreach (var v in viajesDelDia)
        {
            ViajesDisponibles.Add(v);
        }
    }

    private string ObtenerAbreviaturaDia(DateTime fecha)
    {
        return fecha.DayOfWeek switch
        {
            DayOfWeek.Monday => "Lu",
            DayOfWeek.Tuesday => "Ma",
            DayOfWeek.Wednesday => "Mi",
            DayOfWeek.Thursday => "Ju",
            DayOfWeek.Friday => "Vi",
            DayOfWeek.Saturday => "Sa",
            DayOfWeek.Sunday => "Do",
            _ => ""
        };
    }

    // Comando cuando el estudiante toca el botón "Reservar" en un viaje
    [RelayCommand]
    public async Task ReservarViajeAsync(Viaje viajeSeleccionado)
    {
        if (viajeSeleccionado == null || Shell.Current == null) return;

        // Le preguntamos al estudiante dónde quiere que lo recojan
        string puntoRecogida = await Shell.Current.DisplayPromptAsync(
            "Reservar Asiento",
            "¿En qué punto te recogerá el microbús? (Ej. Metrocentro, Pasarela UCA)",
            "Confirmar", "Cancelar");

        if (string.IsNullOrWhiteSpace(puntoRecogida)) return;

        IsLoading = true;

        var authUser = _supabaseService.GetCurrentUser();
        if (authUser == null) return;

        var reserva = new Reserva
        {
            IdViaje = viajeSeleccionado.Id,
            IdPasajero = authUser.Id,
            PuntoRecogidaTexto = puntoRecogida,
            Estado = "pendiente",
            CreadoEn = DateTime.UtcNow
        };

        bool exito = await _supabaseService.CreateReservaAsync(reserva);

        if (exito)
        {
            await Shell.Current.DisplayAlertAsync("¡Reserva Enviada!", "El chofer ha sido notificado. Debes estar en el punto de encuentro a la hora acordada.", "OK");
        }
        else
        {
            await Shell.Current.DisplayAlertAsync("Error", "No se pudo realizar la reserva. Intenta de nuevo.", "OK");
        }

        IsLoading = false;
    }
}