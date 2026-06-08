// PROPÓSITO: DTO de respuesta para POST api/auth/register.
//            Mapea contra RegisterResult del backend.
// PROYECTO: Manda2.Contracts (compartido entre API y MAUI)
// ════════════════════════════════════════════════════════════════════
namespace Manda2.Contracts.Auth;

/// <summary>
/// Respuesta del endpoint de registro.
/// </summary>
public class RegisterResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
}