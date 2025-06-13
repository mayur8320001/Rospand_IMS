using DinkToPdf;
using DinkToPdf.Contracts;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using Rospand_IMS.Data;

using Rospand_IMS.Services;
using System.Text;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllersWithViews();

builder.Services.AddScoped<ISKUGenerator, SKUGenerator>();
/*builder.Services.AddScoped<IPdfService, PdfService>();*/

builder.Services.AddSingleton(typeof(IConverter), new SynchronizedConverter(new PdfTools()));

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("con"),sqlOptions => sqlOptions.CommandTimeout(60)));


builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });
builder.Services.AddAuthorization(options =>
{
    // Category Policies
    options.AddPolicy("CategoryRead", policy =>
        policy.RequireClaim("Permission", "Category:CanRead"));
    options.AddPolicy("CategoryCreate", policy =>
        policy.RequireClaim("Permission", "Category:CanCreate"));
    options.AddPolicy("CategoryUpdate", policy =>
        policy.RequireClaim("Permission", "Category:CanUpdate"));
    options.AddPolicy("CategoryDelete", policy =>
        policy.RequireClaim("Permission", "Category:CanDelete"));

    // Product Policies
    options.AddPolicy("ProductRead", policy =>
        policy.RequireClaim("Permission", "Product:CanRead"));
    options.AddPolicy("ProductCreate", policy =>
        policy.RequireClaim("Permission", "Product:CanCreate"));
    options.AddPolicy("ProductUpdate", policy =>
        policy.RequireClaim("Permission", "Product:CanUpdate"));
    options.AddPolicy("ProductDelete", policy =>
        policy.RequireClaim("Permission", "Product:CanDelete"));

    // Unit Policies
    options.AddPolicy("UnitRead", policy =>
        policy.RequireClaim("Permission", "Unit:CanRead"));
    options.AddPolicy("UnitCreate", policy =>
        policy.RequireClaim("Permission", "Unit:CanCreate"));
    options.AddPolicy("UnitUpdate", policy =>
        policy.RequireClaim("Permission", "Unit:CanUpdate"));
    options.AddPolicy("UnitDelete", policy =>
        policy.RequireClaim("Permission", "Unit:CanDelete"));

    // Payment Terms Policies
    options.AddPolicy("PaymentTermRead", policy =>
        policy.RequireClaim("Permission", "PaymentTerm:CanRead"));
    options.AddPolicy("PaymentTermCreate", policy =>
        policy.RequireClaim("Permission", "PaymentTerm:CanCreate"));
    options.AddPolicy("PaymentTermUpdate", policy =>
        policy.RequireClaim("Permission", "PaymentTerm:CanUpdate"));
    options.AddPolicy("PaymentTermDelete", policy =>
        policy.RequireClaim("Permission", "PaymentTerm:CanDelete"));

    // Tax Policies
    options.AddPolicy("TaxRead", policy =>
        policy.RequireClaim("Permission", "Tax:CanRead"));
    options.AddPolicy("TaxCreate", policy =>
        policy.RequireClaim("Permission", "Tax:CanCreate"));
    options.AddPolicy("TaxUpdate", policy =>
        policy.RequireClaim("Permission", "Tax:CanUpdate"));
    options.AddPolicy("TaxDelete", policy =>
        policy.RequireClaim("Permission", "Tax:CanDelete"));

    // Country/State/City Policies
    options.AddPolicy("CountryRead", policy =>
        policy.RequireClaim("Permission", "Country:CanRead"));
    options.AddPolicy("StateRead", policy =>
        policy.RequireClaim("Permission", "State:CanRead"));
    options.AddPolicy("CityRead", policy =>
        policy.RequireClaim("Permission", "City:CanRead"));

    // Inventory Policies
    options.AddPolicy("InventoryRead", policy =>
        policy.RequireClaim("Permission", "Inventory:CanRead"));
    options.AddPolicy("InventoryAdjust", policy =>
        policy.RequireClaim("Permission", "Inventory:CanAdjust"));
    options.AddPolicy("InventoryLowStock", policy =>
        policy.RequireClaim("Permission", "Inventory:CanViewLowStock"));

    // Customer Policies
    options.AddPolicy("CustomerRead", policy =>
        policy.RequireClaim("Permission", "Customer:CanRead"));
    options.AddPolicy("CustomerCreate", policy =>
        policy.RequireClaim("Permission", "Customer:CanCreate"));
    options.AddPolicy("CustomerUpdate", policy =>
        policy.RequireClaim("Permission", "Customer:CanUpdate"));
    options.AddPolicy("CustomerDelete", policy =>
        policy.RequireClaim("Permission", "Customer:CanDelete"));

    // Sales Order Policies
    options.AddPolicy("SalesOrderRead", policy =>
        policy.RequireClaim("Permission", "SalesOrder:CanRead"));
    options.AddPolicy("SalesOrderCreate", policy =>
        policy.RequireClaim("Permission", "SalesOrder:CanCreate"));
    options.AddPolicy("SalesOrderUpdate", policy =>
        policy.RequireClaim("Permission", "SalesOrder:CanUpdate"));
    options.AddPolicy("SalesOrderDelete", policy =>
        policy.RequireClaim("Permission", "SalesOrder:CanDelete"));

    // Vendor Policies
    options.AddPolicy("VendorRead", policy =>
        policy.RequireClaim("Permission", "Vendor:CanRead"));
    options.AddPolicy("VendorCreate", policy =>
        policy.RequireClaim("Permission", "Vendor:CanCreate"));
    options.AddPolicy("VendorUpdate", policy =>
        policy.RequireClaim("Permission", "Vendor:CanUpdate"));
    options.AddPolicy("VendorDelete", policy =>
        policy.RequireClaim("Permission", "Vendor:CanDelete"));

    // Purchase Order Policies
    options.AddPolicy("PurchaseOrderRead", policy =>
        policy.RequireClaim("Permission", "PurchaseOrder:CanRead"));
    options.AddPolicy("PurchaseOrderCreate", policy =>
        policy.RequireClaim("Permission", "PurchaseOrder:CanCreate"));
    options.AddPolicy("PurchaseOrderUpdate", policy =>
        policy.RequireClaim("Permission", "PurchaseOrder:CanUpdate"));
    options.AddPolicy("PurchaseOrderDelete", policy =>
        policy.RequireClaim("Permission", "PurchaseOrder:CanDelete"));
    options.AddPolicy("PurchaseOrderReceive", policy =>
        policy.RequireClaim("Permission", "PurchaseOrder:CanReceive"));

    // User Policies
    options.AddPolicy("UserRead", policy =>
    policy.RequireClaim("Permission", "Auth:CanRead"));
    options.AddPolicy("UserCreate", policy =>
        policy.RequireClaim("Permission", "Auth:CanCreate"));

    options.AddPolicy("RoleRead", policy =>
        policy.RequireClaim("Permission", "Role:CanRead"));
});



builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IPermissionService, PermissionService>();


var app = builder.Build();

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
app.Use(async (context, next) =>
{
    var token = context.Request.Cookies["jwtToken"];
    if (!string.IsNullOrEmpty(token))
    {
        context.Request.Headers.Authorization = $"Bearer {token}";
    }
    await next();
});

app.UseAuthentication();
app.UseAuthorization();

// Proper MVC default routing
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");


app.Run();
