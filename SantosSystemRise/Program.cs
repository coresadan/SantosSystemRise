using SantosSystemRise.Components;
using SantosSystemRise.Services;
using Microsoft.EntityFrameworkCore;
using SantosSystemRise.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// 1. Registrar EF Core con SQLite
builder.Services.AddDbContext<SystemRiseContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("SystemRiseDb")));

// 2. Registrar tu servicio como Scoped (correcto para EF Core)
builder.Services.AddScoped<RiseControlService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseAntiforgery();
app.MapStaticAssets();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
