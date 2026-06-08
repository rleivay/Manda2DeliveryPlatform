using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Application.DTOs
{
    public record DownloadDocumentResponse(string FileName, byte[] Content, string ContentType);
}
