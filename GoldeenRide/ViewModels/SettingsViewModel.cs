using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GoldeenRide.Models;
using GoldeenRide.Services;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.ApplicationModel.DataTransfer;
using System;

namespace GoldeenRide.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly SupabaseService _supabaseService = SupabaseService.Instance;

    [ObservableProperty] private string userName = "Cargando...";
    [ObservableProperty] private string userRole = "";

    [ObservableProperty] private bool isFleetManager = false;
    [ObservableProperty] private bool isEmployee = false;
    [ObservableProperty] private bool hasBoss = false;

    [ObservableProperty] private string myFleetCode = "Cargando...";
    [ObservableProperty] private string inputFleetCode = "";
    [ObservableProperty] private string currentBossName = "";

    [ObservableProperty] private bool isLoading = false;

    private Usuario? _currentUserData;

    public SettingsViewModel()
    {
        _ = LoadProfileAsync();
    }

    // Helper para sacar las traducciones desde el código C#
    private string GetTranslation(string key)
    {
        if (Application.Current != null && Application.Current.Resources.TryGetValue(key, out var value))
            return value?.ToString() ?? key;
        return key;
    }

    [RelayCommand]
    public async Task LoadProfileAsync()
    {
        IsLoading = true;
        var authUser = _supabaseService.GetCurrentUser();
        if (authUser != null && !string.IsNullOrEmpty(authUser.Id))
        {
            _currentUserData = await _supabaseService.GetUserDataAsync(authUser.Id);
            if (_currentUserData != null)
            {
                UserName = _currentUserData.Nombre;
                UserRole = _currentUserData.Rol;

                string rolLower = UserRole.ToLower();
                IsFleetManager = rolLower.Contains("jefe") || rolLower.Contains("owner");
                IsEmployee = rolLower.Contains("empleado");

                if (IsFleetManager)
                {
                    MyFleetCode = _currentUserData.Id;
                }

                if (IsEmployee && !string.IsNullOrEmpty(_currentUserData.IdJefe))
                {
                    HasBoss = true;
                    var jefe = await _supabaseService.GetUserDataAsync(_currentUserData.IdJefe);
                    CurrentBossName = jefe?.Nombre ?? "Jefe Desconocido";
                }
            }
        }
        IsLoading = false;
    }

    [RelayCommand]
    public async Task CopyFleetCodeAsync()
    {
        await Clipboard.Default.SetTextAsync(MyFleetCode);
        if (Shell.Current != null)
            await Shell.Current.DisplayAlertAsync(
                GetTranslation("AlertCopiedTitle"),
                GetTranslation("AlertCopiedMsg"),
                GetTranslation("AlertOk"));
    }

    [RelayCommand]
    public async Task JoinFleetAsync()
    {
        if (string.IsNullOrWhiteSpace(InputFleetCode))
        {
            if (Shell.Current != null)
                await Shell.Current.DisplayAlertAsync(GetTranslation("AlertAttention"), GetTranslation("AlertEmptyCode"), GetTranslation("AlertOk"));
            return;
        }

        if (_currentUserData == null || Shell.Current == null) return;

        IsLoading = true;
        var (success, message) = await _supabaseService.JoinFleetByCodeAsync(InputFleetCode.Trim(), _currentUserData.Id);

        if (success)
        {
            await Shell.Current.DisplayAlertAsync(GetTranslation("AlertWelcome"), message, GetTranslation("AlertOk"));
            await LoadProfileAsync();
        }
        else
        {
            await Shell.Current.DisplayAlertAsync(GetTranslation("AlertError"), message, GetTranslation("AlertOk"));
        }
        IsLoading = false;
    }

    [RelayCommand]
    public async Task LogoutAsync()
    {
        if (Shell.Current == null) return;

        bool confirm = await Shell.Current.DisplayAlertAsync(
            GetTranslation("AlertLogoutTitle"),
            GetTranslation("AlertLogoutMsg"),
            GetTranslation("AlertYesExit"),
            GetTranslation("AlertCancel"));

        if (confirm)
        {
            IsLoading = true;
            await _supabaseService.LogoutAsync();
            IsLoading = false;

            AppShellViewModel.Instance.ResetMenu(); // Ocultamos el menú al salir
            await Shell.Current.GoToAsync("///login");
        }
    }
}