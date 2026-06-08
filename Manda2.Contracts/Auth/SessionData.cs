// PROPÓSITO: Modelo local de sesión activa en la app MAUI.
//            NO es un DTO del backend. Se construye a partir de LoginResponse
//            y se persiste en SecureStorage del dispositivo.
// PROYECTO: Manda2.Contracts (compartido entre API y MAUI)
// ════════════════════════════════════════════════════════════════════
namespace Manda2.Contracts.Auth;

/// <summary>
/// Datos de sesión del usuario autenticado.
/// Se almacena localmente en SecureStorage tras un login exitoso.
/// </summary>
public class SessionData
{
    /// <summary>ID del AppUser en la base de datos.</summary>
    public int UserId { get; set; }

    /// <summary>Nombre completo del usuario (FirstName + LastName).</summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>Email del usuario autenticado.</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>Rol del usuario: Customer, Driver o Merchant.</summary>
    public string Role { get; set; } = string.Empty;

    /// <summary>JWT de corta duración para llamadas a la API.</summary>
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>Token opaco para renovar el AccessToken.</summary>
    public string RefreshToken { get; set; } = string.Empty;

    /// <summary>Fecha de expiración del AccessToken (UTC).</summary>
    public DateTime? AccessTokenExpiresAt { get; set; }
}