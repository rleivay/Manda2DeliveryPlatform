// PROPÓSITO: DTO de entrada para POST api/auth/register desde la app MAUI.
//            Mapea exactamente contra RegisterCommand del backend.
// PROYECTO: Manda2.Contracts (compartido entre API y MAUI)
// ════════════════════════════════════════════════════════════════════
namespace Manda2.Contracts.Auth;

/// <summary>
/// Request de registro de nuevo usuario.
/// El campo Role se fija en "Customer" desde la app MAUI.
/// Driver y Merchant se registran desde BackOffice.
/// </summary>
public class RegisterRequest
{
    /// <summary>Email del nuevo usuario.</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>Contraseña en texto plano (el backend la hashea con BCrypt).</summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Rol solicitado. Desde la app MAUI siempre será "Customer".
    /// Valores válidos en backend: Customer, Driver, Merchant.
    /// </summary>
    public string Role { get; set; } = "Customer";

    /// <summary>Nombre del usuario.</summary>
    public string? FirstName { get; set; }

    /// <summary>Apellido del usuario.</summary>
    public string? LastName { get; set; }

    /// <summary>Teléfono de contacto.</summary>
    public string? Phone { get; set; }
}
