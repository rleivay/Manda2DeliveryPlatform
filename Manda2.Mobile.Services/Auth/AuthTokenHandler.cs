using Manda2.Mobile.Services.Session;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Mobile.Services.Auth
{
    //public class AuthTokenHandler : DelegatingHandler
    //{
    //    private readonly ISessionService _session;

    //    public AuthTokenHandler(ISessionService session)
    //    {
    //        _session = session;
    //    }

    //    protected override async Task<HttpResponseMessage> SendAsync(
    //        HttpRequestMessage request,
    //        CancellationToken cancellationToken)
    //    {
    //        var token = _session.GetToken(); // o como expongas el token

    //        if (!string.IsNullOrWhiteSpace(token))
    //        {
    //            request.Headers.Authorization =
    //                new AuthenticationHeaderValue("Bearer", token);
    //        }

    //        return await base.SendAsync(request, cancellationToken);
    //    }
    //}
}
