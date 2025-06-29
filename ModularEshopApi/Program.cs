using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using ModularEshopApi.Data;
using System.Globalization;
using ModularEshopApi.Helpers;

var builder = WebApplication.CreateBuilder(args);
var myAllowedOrigins = "_myAllowedOrigins";

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddAutoMapper(typeof(MappingProfile));

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: myAllowedOrigins,
        builder =>
        {
            builder.WithOrigins("http://localhost:5173",
            "http://192.168.0.29:5173")
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        });
});

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Modular Eshop API",
        Version = "v1",
        Description = "API for managing users and products in a modular webshop.",
    });
});


builder.Services.AddDbContext<ApiDbContext>(opt => opt.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddControllers().AddNewtonsoftJson();

var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Modular Eshop API v1");
        options.RoutePrefix = "swagger";
    });
}

var cultureInfo = CultureInfo.InvariantCulture;
CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

app.UseRouting();

app.UseCors(myAllowedOrigins);

app.UseStaticFiles();

app.UseAuthorization();

app.MapControllers();

app.Run();
