using DocumentFrontend.Components;
using DocumentFrontend.Services;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();


builder.Services.AddScoped<TokenService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<ApiAuthenticationStateProvider>();
builder.Services.AddSingleton<DocumentService>();

builder.Services.AddTransient<AuthHandler>();

builder.Services.AddHttpClient("Authenticated", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiGateway__BasePath"]!);
})
.AddHttpMessageHandler<AuthHandler>();

builder.Services.AddHttpClient("Unauthenticated", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiGateway__BasePath"]!);
});

builder.Services.AddAuthentication("Cookies")
    .AddCookie("Cookies", options =>
    {
        options.LoginPath = "/login";
        options.AccessDeniedPath = "/access-denied";
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
