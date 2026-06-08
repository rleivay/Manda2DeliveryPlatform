using Manda2.Contracts.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Contracts.Catalog
{
    public record MerchantDto(
        int Id,
        string Name,
        string CommercialPhone,
        string CommercialEmail,
        string? IdentityDocumentUrl,
        bool IsActive,
        MerchantType Type,
        string? BankAccountNumber,
        string? BankAccountType,
        string? BankName,
        string? LogoUrl,
        DateTime CreatedAt,
        DateTime? UpdatedAt
    );
}
