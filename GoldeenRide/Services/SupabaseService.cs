using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using GoldeenRide.Models;
using Microsoft.Maui.Storage;
using Newtonsoft.Json;
using Supabase;
using Supabase.Gotrue;
using Session = Supabase.Gotrue.Session;
using SupabaseClient = Supabase.Client;
using GotrueUser = Supabase.Gotrue.User;

namespace GoldeenRide.Services;

public class SupabaseService
{
    private const string SupabaseUrl = "https://";
    private const string SupabaseAnonKey = "key supabase";

    private static SupabaseService? _instance;
    private SupabaseClient? _supabaseClient;
    private Session? _currentSession;
    private bool _isInitialized = false;

    public static SupabaseService Instance => _instance ??= new SupabaseService();
    public Session? CurrentSession => _currentSession;
    public bool IsAuthenticated => _currentSession != null;

    public SupabaseService() { }

    private async Task EnsureInitializedAsync()
    {
        if (_isInitialized) return;
        try
        {
            var options = new SupabaseOptions { AutoRefreshToken = true, AutoConnectRealtime = false };
            _supabaseClient = new SupabaseClient(SupabaseUrl, SupabaseAnonKey, options);
            await _supabaseClient.InitializeAsync();
            _isInitialized = true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"❌ Error conectando Supabase: {ex.Message}");
            throw;
        }
    }

    // ─── SESIÓN PERMANENTE  ───
    public async Task<bool> IsSessionActiveAsync()
    {
        try
        {
            await EnsureInitializedAsync();
            string? savedSessionJson = await SecureStorage.Default.GetAsync("supabase_session");

            if (!string.IsNullOrEmpty(savedSessionJson))
            {
                var session = JsonConvert.DeserializeObject<Session>(savedSessionJson);
                if (session != null && !string.IsNullOrEmpty(session.AccessToken) && !string.IsNullOrEmpty(session.RefreshToken))
                {
                    
                    _currentSession = await _supabaseClient!.Auth.SetSession(session.AccessToken!, session.RefreshToken!);
                    return true;
                }
            }
            return false;
        }
        catch { return false; }
    }

    public async Task<(bool Success, string Message)> RegisterAsync(string email, string password, string nombre, string rol, string fotoPerfil)
    {
        try
        {
            await EnsureInitializedAsync();
            if (_supabaseClient == null) return (false, "No se pudo conectar");

            var options = new SignUpOptions { Data = new Dictionary<string, object> { { "nombre", nombre }, { "rol", rol }, { "foto_perfil", fotoPerfil } } };
            await _supabaseClient.Auth.SignUp(email, password, options);
            return (true, "Registro exitoso.");
        }
        catch (Exception ex) { return (false, ex.Message); }
    }

    public async Task<(bool Success, string Message, Session? Session)> LoginAsync(string email, string password)
    {
        try
        {
            await EnsureInitializedAsync();
            if (_supabaseClient == null) return (false, "Error de conexión", null);
            var session = await _supabaseClient.Auth.SignInWithPassword(email, password);
            _currentSession = session;

            // Guardamos la sesión en la caja fuerte del celular
            string sessionJson = JsonConvert.SerializeObject(session);
            await SecureStorage.Default.SetAsync("supabase_session", sessionJson);

            return (true, "Sesión iniciada", session);
        }
        catch (Exception) { return (false, "Correo o contraseña incorrectos.", null); }
    }

    public async Task<bool> LogoutAsync()
    {
        try
        {
            await EnsureInitializedAsync();
            if (_supabaseClient == null) return false;
            await _supabaseClient.Auth.SignOut();
            _currentSession = null;
            SecureStorage.Default.Remove("supabase_session"); // Borra la sesión al salir
            return true;
        }
        catch { return false; }
    }

    public GotrueUser? GetCurrentUser() => _supabaseClient?.Auth.CurrentUser;

    public async Task<Usuario?> GetUserDataAsync(string userId)
    {
        try
        {
            await EnsureInitializedAsync();
            if (_supabaseClient == null || string.IsNullOrEmpty(userId)) return null;
            return await _supabaseClient.From<Usuario>().Where(x => x.Id == userId).Single();
        }
        catch { return null; }
    }

    public async Task<bool> SaveUserDataAsync(string userId, string nombre, string rol, string? fotoPerfil = null)
    {
        try
        {
            await EnsureInitializedAsync();
            if (_supabaseClient == null || string.IsNullOrEmpty(userId)) return false;
            var usuario = await _supabaseClient.From<Usuario>().Where(x => x.Id == userId).Single();
            if (usuario != null)
            {
                usuario.Nombre = nombre; usuario.Rol = rol; usuario.FotoPerfil = fotoPerfil;
                await usuario.Update<Usuario>();
                return true;
            }
            return false;
        }
        catch { return false; }
    }

    public async Task<List<Vehiculo>> GetVehiclesByOwnerAsync(string ownerId)
    {
        try
        {
            await EnsureInitializedAsync();
            if (_supabaseClient == null || string.IsNullOrEmpty(ownerId)) return new List<Vehiculo>();
            var response = await _supabaseClient.From<Vehiculo>().Where(v => v.IdPropietario == ownerId).Get();
            return response.Models;
        }
        catch { return new List<Vehiculo>(); }
    }

    public async Task<List<Usuario>> GetEmployeesByBossAsync(string bossId)
    {
        try
        {
            await EnsureInitializedAsync();
            if (_supabaseClient == null || string.IsNullOrEmpty(bossId)) return new List<Usuario>();
            var response = await _supabaseClient.From<Usuario>().Where(u => u.IdJefe == bossId).Get();
            return response.Models;
        }
        catch { return new List<Usuario>(); }
    }

    public async Task<bool> AddVehicleAsync(Vehiculo vehiculo)
    {
        try
        {
            await EnsureInitializedAsync();
            if (_supabaseClient == null) return false;
            await _supabaseClient.From<Vehiculo>().Insert(vehiculo);
            return true;
        }
        catch { return false; }
    }

    public async Task<(bool Success, string Message)> LinkEmployeeByEmailAsync(string emailEmpleado, string idJefe)
    {
        try
        {
            await EnsureInitializedAsync();
            if (_supabaseClient == null) return (false, "Error de conexión");
            var response = await _supabaseClient.From<Usuario>().Where(u => u.Email == emailEmpleado).Get();
            var empleado = response.Models.FirstOrDefault();
            if (empleado == null) return (false, "No se encontró el correo.");
            if (!empleado.Rol.Contains("empleado")) return (false, "Este usuario no es Chofer Empleado.");
            if (!string.IsNullOrEmpty(empleado.IdJefe)) return (false, "Ya trabaja para otro jefe.");
            empleado.IdJefe = idJefe;
            await empleado.Update<Usuario>();
            return (true, $"¡Vincualdo con éxito!");
        }
        catch (Exception ex) { return (false, ex.Message); }
    }

    public async Task<bool> CreateTripAsync(Viaje viaje)
    {
        try
        {
            await EnsureInitializedAsync();
            if (_supabaseClient == null) return false;
            await _supabaseClient.From<Viaje>().Insert(viaje);
            return true;
        }
        catch { return false; }
    }


    // ───UNIRSE A FLOTA POR CÓDIGO ───
    public async Task<(bool Success, string Message)> JoinFleetByCodeAsync(string bossId, string employeeId)
    {
        try
        {
            await EnsureInitializedAsync();
            if (_supabaseClient == null) return (false, "Error de conexión.");

            // 1. Verificar si el código (ID del jefe) es válido
            var bossResponse = await _supabaseClient.From<Usuario>().Where(u => u.Id == bossId).Get();
            var boss = bossResponse.Models.FirstOrDefault();

            if (boss == null || !boss.Rol.ToLower().Contains("jefe"))
                return (false, "El código ingresado no pertenece a un Chofer Jefe válido.");

            // 2. Actualización a prueba de falsos errores
            try
            {
                await _supabaseClient.From<Usuario>()
                    .Where(u => u.Id == employeeId)
                    .Set(u => u.IdJefe, boss.Id)
                    .Update();
            }
            catch (Exception updateEx) when (updateEx.Message.Contains("Sequence contains no elements"))
            {
                
            }

            return (true, $"¡Te has unido con éxito a la flota de {boss.Nombre}!");
        }
        catch (Exception ex)
        {
            return (false, $"Error al unirse: Asegúrate que el código no tenga espacios. Detalle: {ex.Message}");
        }
    }
    // ─── FUNCIONES PARA EL PASAJERO (ESTUDIANTE) ───

    public async Task<List<Viaje>> GetAllActiveTripsAsync()
    {
        try
        {
            await EnsureInitializedAsync();
            if (_supabaseClient == null) return new List<Viaje>();

            // Traemos los viajes que están programados
            var response = await _supabaseClient.From<Viaje>().Where(v => v.Estado == "programado").Get();
            return response.Models;
        }
        catch { return new List<Viaje>(); }
    }

    public async Task<bool> CreateReservaAsync(Reserva reserva)
    {
        try
        {
            await EnsureInitializedAsync();
            if (_supabaseClient == null) return false;

            await _supabaseClient.From<Reserva>().Insert(reserva);
            return true;
        }
        catch { return false; }
    }
}