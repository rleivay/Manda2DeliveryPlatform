using Manda2.Contracts.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Mobile.Services.Session
{
    /// <summary>
    /// Servicio de sesión local.
    /// Guarda y recupera la sesión autenticada desde SecureStorage.
    /// </summary>
    public interface ISessionService
    {

        /// <summary>
        /// Suscribirse para recibir notificación cuando la sesión cambia.
        /// El string? es el nuevo Role (null = sesión cerrada).
        /// </summary>
        event Action<string?> OnSessionChanged;

        /// <summary>
        /// Persiste la sesión en SecureStorage a partir de un LoginResponse exitoso.
        /// </summary>
        Task SaveSessionAsync(LoginResponse loginResponse, CancellationToken ct = default);

        /// <summary>
        /// Retorna los datos de sesión activa. Null si no hay sesión.
        /// </summary>
        Task<SessionData?> GetSessionAsync(CancellationToken ct = default);

        /// <summary>
        /// Elimina la sesión del SecureStorage (logout local).
        /// </summary>
        Task ClearSessionAsync(CancellationToken ct = default);

        /// <summary>
        /// Indica si hay una sesión activa con token no expirado.
        /// </summary>
        Task<bool> IsAuthenticatedAsync(CancellationToken ct = default);

        /// <summary>
        /// Configuración de negocio activa para esta sesión.
        /// Null hasta que se llame StartSessionAsync().
        /// </summary>
        SessionConfig? Config { get; }

        /// <summary>
        /// ID de la dirección seleccionada para la operativa actual.
        /// Solo vive en memoria.
        /// </summary>
        int? SelectedShippingAddressId { get; set; }

        /// <summary>
        /// Información resumida de la dirección actual para la UI.
        /// </summary>
        string? SelectedAddressText { get; set; }

        /// <summary>
        /// Carga configuraciones de negocio desde el BackEnd post-login.
        /// Debe llamarse una vez tras SaveSessionAsync exitoso.
        /// </summary>
        /// 
        Task StartSessionAsync(CancellationToken ct = default);
    }
}
