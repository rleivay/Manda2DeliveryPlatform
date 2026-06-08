using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Application.DTOs
{
    public record UploadIdentityDocumentRequest(
        int ActorId,
        string ActorType, // "Customer", "Merchant", "Driver"
        IFormFile Document
    );
}
