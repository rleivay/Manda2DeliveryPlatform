using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Application.DTOs
{
    public record CustomerDto(
        int Id,
        string FirstName,
        string LastName,
        string Email,
        string PhoneNumber,
        string? IdentityDocumentUrl,
        bool IsActive,
        DateTime CreatedAt,
        DateTime? UpdatedAt
    );
}
