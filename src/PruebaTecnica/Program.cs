using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TriviaApp.Data;
using TriviaApp.Services;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("TriviaBAC")
    ?? throw new InvalidOperationException("Falta la cadena de conexión 'CertiBAC' en appsettings.json.");

// ---------- MVC + protección CSRF en todas las peticiones POST ----------
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
});

// Las llamadas AJAX envían el token en este encabezado.
builder.Services.AddAntiforgery(options => options.HeaderName = "RequestVerificationToken");

// ---------- Autenticación por cookie ----------
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/Login";
        options.Cookie.Name = "TriviaBAC.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.ExpireTimeSpan = TimeSpan.FromHours(2);
        options.SlidingExpiration = true;

        // Para peticiones AJAX responder 401 en lugar de redirigir al login (HTML).
        options.Events.OnRedirectToLogin = context =>
        {
            if (context.Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return Task.CompletedTask;
            }

            context.Response.Redirect(context.RedirectUri);
            return Task.CompletedTask;
        };
    });

// Seguro por defecto: todo requiere sesión salvo lo marcado con [AllowAnonymous].
builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

// ---------- Límite de intentos en login / registro (fuerza bruta) ----------
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("auth", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "desconocido",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 20,
                Window = TimeSpan.FromMinutes(1)
            }));
});

// ---------- Inyección de dependencias ----------
builder.Services.AddSingleton<IStoredProcedureRunner>(_ => new SqlStoredProcedureRunner(connectionString));
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<ITriviaRepository, TriviaRepository>();
builder.Services.AddSingleton<IPasswordHasher>(_ => new Pbkdf2PasswordHasher());
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<TriviaService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
