// DriverDto.cs
//
// Objetivo:
// - Representar solo los datos necesarios para la API
// - Evitar exponer entidades del dominio directamente
// - Facilitar serialización segura

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Application.DTOs
{
    /// <summary>
    /// DTO de solo lectura para representar un repartidor en respuestas API.
    /// </summary>
    /// <param name="Id">Identificador único</param>
    /// <param name="FirstName">Nombre</param>
    /// <param name="LastName">Apellido</param>
    /// <param name="Email">Correo electrónico</param>
    /// <param name="PhoneNumber">Teléfono</param>
    /// <param name="IsActive">Estado activo</param>
    /// <param name="MaxCashLimit">Límite máximo de efectivo permitido</param>
    /// <param name="CurrentCashBalance">Saldo actual en efectivo</param>
    /// <param name="IsCashBlocked">Indica si está bloqueado por límite de efectivo</param>
    /// <param name="MaxActiveGroups">Número máximo de grupos de entrega activos</param>
    public record DriverDto(
    int Id,
    string FirstName,
    string LastName,
    string Email,
    string PhoneNumber,
    string IdentityDocumentUrl,
    bool IsActive,
    decimal MaxCashLimit,
    decimal CurrentCashBalance,
    bool IsCashBlocked,
    int MaxActiveGroups,
    string? BankAccountNumber,
    string? BankAccountType,
    string? BankName,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

    
}
