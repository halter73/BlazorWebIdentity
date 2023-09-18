using System.Reflection;
using BlazorWeb;
using BlazorWeb.Components;
using BlazorWeb.Identity;
using BlazorWeb.Identity.Data;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

using var sqlConnection = new SqliteConnection("DataSource=:memory:");
sqlConnection.Open();

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<BlazorWebContext>(options => options.UseSqlite(sqlConnection));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddAuthentication(IdentityConstants.ApplicationScheme)
    .AddIdentityCookies();

builder.Services.ConfigureApplicationCookie(cookieOptions =>
{
    cookieOptions.LoginPath = new("/Identity/Account/Login");
});

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<AuthenticationStateProvider, ServerAuthenticationStateProvider>();
builder.Services.AddScoped<UserAccessor>();
builder.Services.AddSingleton<IEmailSender, NoOpEmailSender>();

builder.Services.AddIdentityCore<BlazorWebUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<BlazorWebContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

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

app.MapIdentityUI();

app.MapRazorComponents<App>()
    .AddServerRenderMode()
    .AddWebAssemblyRenderMode()
    .AddAdditionalAssemblies(Assembly.Load("BlazorWeb.Client"));

using (var scope = app.Services.CreateScope())
{
    scope.ServiceProvider.GetRequiredService<BlazorWebContext>().Database.EnsureCreated();
}

app.Run();
