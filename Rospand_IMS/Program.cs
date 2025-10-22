using DinkToPdf;
using DinkToPdf.Contracts;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using QuestPDF.Infrastructure;
using Rospand_IMS.Data;
using Rospand_IMS.Services;

var builder = WebApplication.CreateBuilder(args);

// Custom services
builder.Services.AddScoped<ISKUGenerator, SKUGenerator>();
builder.Services.AddSingleton<IConverter>(new SynchronizedConverter(new PdfTools()));
builder.Services.AddScoped<PdfService>();

// ✅ Use only ONE connection string (choose one of the following)
// Option 1: LocalDB with Windows Authentication (recommended for development)
var connectionString = builder.Configuration.GetConnectionString("con") 
    ?? builder.Configuration.GetConnectionString("SqlConnection")
    ?? throw new InvalidOperationException("Connection string not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString, sqlServerOptionsAction: sqlOptions =>
    {
        sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorNumbersToAdd: null);
    }));

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// Excel and PDF licensing
ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
QuestPDF.Settings.License = LicenseType.Community;

// Build app
var app = builder.Build();

// Error handling
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

// Middleware
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// Routes
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();