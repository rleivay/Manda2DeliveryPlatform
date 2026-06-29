using CommunityToolkit.Maui;
using Manda2.Mobile.Services.Auth;
using Manda2.Mobile.Services.Cart;
using Manda2.Mobile.Services.Catalog;
using Manda2.Mobile.Services.CheckOut;
using Manda2.Mobile.Services.Orders;
using Manda2.Mobile.Services.Session;
using Manda2.Mobile.Services.ShippingAddress;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace Manda2.Mobile.App
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();

            // ── Cargar appsettings.json embebido ─────────────────────────────────────
            var assembly = Assembly.GetExecutingAssembly();
            using var stream = assembly.GetManifestResourceStream("Manda2.Mobile.App.appsettings.json");

            if (stream != null)
            {
                var config = new ConfigurationBuilder()
                    .AddJsonStream(stream)
                    .Build();
                builder.Configuration.AddConfiguration(config);
            }

            // ── Leer URL base desde configuración ────────────────────────────────────
            var apiBaseUrl = builder.Configuration["ApiSettings:BaseUrl"]
                             ?? "https://192.168.100.67:7255/";

            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });

            builder.Services.AddMauiBlazorWebView();

            // ─── Factory de handler SSL (solo DEBUG) ─────────────────────────────────
#if DEBUG
            static HttpClientHandler CreateDevHandler() => new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback =
                    HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            };
#endif

            // ─── AuthService ─────────────────────────────────────────────────────────
            builder.Services.AddHttpClient<IAuthService, AuthService>(client =>{client.BaseAddress = new Uri(apiBaseUrl);})
#if DEBUG
            .ConfigurePrimaryHttpMessageHandler(CreateDevHandler);
#endif

            // ─── CatalogService ──────────────────────────────────────────────────────
            builder.Services.AddHttpClient<ICatalogService, CatalogService>(client =>{client.BaseAddress = new Uri(apiBaseUrl);})
#if DEBUG
            .ConfigurePrimaryHttpMessageHandler(CreateDevHandler);
#endif

            // ─── HttpClient genérico para descarga de imágenes ───────────────────────
            // Usado por MerchantDetail para convertir imágenes a base64.
            builder.Services.AddHttpClient("ImageClient", client =>
            {
                client.BaseAddress = new Uri(apiBaseUrl);
            })
#if DEBUG
            .ConfigurePrimaryHttpMessageHandler(CreateDevHandler);
#endif

            // ─── Servicios de Checkout / Orders ──────────────────────────────────────
            builder.Services.AddHttpClient<IOrderGroupApiService, OrderGroupApiService>(client =>
            {
                client.BaseAddress = new Uri(apiBaseUrl);
            })
#if DEBUG
            .ConfigurePrimaryHttpMessageHandler(CreateDevHandler);
#endif

            builder.Services.AddHttpClient<IShippingAddressApiService, ShippingAddressApiService>(client =>
            {
                client.BaseAddress = new Uri(apiBaseUrl);
            })
#if DEBUG
            .ConfigurePrimaryHttpMessageHandler(CreateDevHandler);
#endif

            builder.Services.AddHttpClient<IPaymentMethodApiService, PaymentMethodApiService>(client =>
            {
                client.BaseAddress = new Uri(apiBaseUrl);
            })
#if DEBUG
            .ConfigurePrimaryHttpMessageHandler(CreateDevHandler);
#endif

            builder.Services.AddHttpClient<ICheckoutApiService, CheckoutApiService>(client =>
            {
                client.BaseAddress = new Uri(apiBaseUrl);
            })
#if DEBUG
            .ConfigurePrimaryHttpMessageHandler(CreateDevHandler);
#endif


            // ─── Servicios de estado local / UI ──────────────────────────────────────
            builder.Services.AddSingleton<ICartService, CartService>();
            
            // ─── Servicios de autenticación y sesión ─────────────────────────────────
            builder.Services.AddSingleton<ISessionService, SessionService>();

#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
