using System.Reflection;
using BlazorWeb.Components;
using BlazorWeb.Identity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

using var sqlConnection = new SqliteConnection("DataSource=:memory:");
sqlConnection.Open();

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<BlazorIdentityContext>(options => options.UseSqlite(sqlConnection));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddAuthentication()
    .AddIdentityCookies();

builder.Services.AddCascadingAuthenticationState();

builder.Services.AddIdentityCore<BlazorIdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddApiEndpoints()
    .AddEntityFrameworkStores<BlazorIdentityContext>();

builder.Services.AddRazorComponents()
    .AddServerComponents()
    .AddWebAssemblyComponents();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.MapRazorComponents<App>()
    .AddServerRenderMode()
    .AddWebAssemblyRenderMode()
    .AddAdditionalAssemblies(Assembly.Load("BlazorWeb.Client"));

using (var scope = app.Services.CreateScope())
{
    scope.ServiceProvider.GetRequiredService<BlazorIdentityContext>().Database.EnsureCreated();
}

app.Run();
