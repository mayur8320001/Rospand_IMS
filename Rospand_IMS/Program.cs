using DinkToPdf;
using DinkToPdf.Contracts;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using QuestPDF.Infrastructure;
using Rospand_IMS.Areas.Identity.Data;
using Rospand_IMS.Data;
using Rospand_IMS.Models;

using Rospand_IMS.Services;
using System.Text;

var builder = WebApplication.CreateBuilder(args);




builder.Services.AddScoped<ISKUGenerator, SKUGenerator>();
// In ConfigureServices method
builder.Services.AddSingleton(typeof(IConverter), new SynchronizedConverter(new PdfTools()));
builder.Services.AddScoped<PdfService>();

builder.Services.AddSingleton(typeof(IConverter), new SynchronizedConverter(new PdfTools()));

var connectionString = builder.Configuration.GetConnectionString("con") ?? throw new InvalidOperationException("Connection string 'AuthDbContextConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connectionString));

builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddEntityFrameworkStores<ApplicationDbContext>();

/*builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("con")));*/

builder.Services.AddControllersWithViews();

QuestPDF.Settings.License = LicenseType.Community; // or LicenseType.Commercial

builder.Services.AddRazorPages();

builder.Services.Configure<IdentityOptions>(options =>
{
    options.Password.RequireUppercase = false;
});


  
var app = builder.Build();


//await SeedService.SeedDatabase(app.Services);

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}


app.UseDeveloperExceptionPage();

// Serve static files and handle routing
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();


app.UseAuthentication();
app.UseAuthorization();

// Proper MVC default routing
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();
app.Run();
