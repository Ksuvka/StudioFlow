using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using StudioFlow.Components;
using StudioFlow.Data;
using StudioFlow.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();


builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<AuthStateService>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<AuthStateService>());
builder.Services.AddScoped<AuthStateService>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<AuthStateService>());
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<AppStateService>();
builder.Services.AddScoped<AuthService>();



var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    dbContext.Database.Migrate();
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();