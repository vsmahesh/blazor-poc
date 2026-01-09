using BasicBlazor.Web.Components;
using BasicBlazor.Web.Authorization;
using BasicBlazor.Data.Data;
using BasicBlazor.Data.Extensions;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;

var builder = WebApplication.CreateBuilder(args);

// Discover extension assemblies early for proper routing and component registration
// This must happen before AddRazorComponents() to ensure extension pages are included in routing table
var extensionAssemblies = ExtensionLoader.GetExtensionAssemblies();

// Read session timeout configuration
var sessionTimeoutMinutes = builder.Configuration.GetValue<int>("SessionTimeout:TimeoutMinutes", 5);
var sessionTimeoutEnabled = builder.Configuration.GetValue<bool>("SessionTimeout:Enabled", true);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add HttpContextAccessor for accessing HttpContext in components
builder.Services.AddHttpContextAccessor();

// Register all Data layer services with extension support (DbContext, repositories, business services)
// This discovers and loads client extensions (BasicBlazor.Extension.*) if present
builder.Services.AddDataServicesWithExtensions(builder.Configuration);

// Add authentication services
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/login";
        options.LogoutPath = "/logout";
        options.AccessDeniedPath = "/access-denied";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.ExpireTimeSpan = TimeSpan.FromMinutes(sessionTimeoutMinutes);
        options.SlidingExpiration = true;
    });

// Add authorization with custom RoleAccess policy
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RoleAccess", policy =>
        policy.Requirements.Add(new RoleAccessRequirement()));
});
builder.Services.AddCascadingAuthenticationState();

// Register custom authorization handler
builder.Services.AddSingleton<IAuthorizationHandler, RoleAccessHandler>();

// Add antiforgery for form POST security
builder.Services.AddAntiforgery();

// Register permission authorization handler (Scoped - needs DbContext)
builder.Services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();

// Register custom policy provider for dynamic permission policies
builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

// Add authentication and authorization middleware (ORDER IS CRITICAL)
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddAdditionalAssemblies(extensionAssemblies);

// Apply migrations and seed database on startup (PoC only - use explicit migrations in production)
app.Services.ApplyDataMigrations();

// Session keep-alive endpoint
app.MapPost("/api/session/keep-alive", async (HttpContext context) =>
{
    // Sliding expiration automatically extends cookie on authenticated request
    return Results.Ok(new
    {
        success = true,
        message = "Session extended",
        expiresIn = sessionTimeoutMinutes * 60
    });
})
.RequireAuthorization();

app.Run();
