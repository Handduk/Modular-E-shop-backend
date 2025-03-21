using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using ModularEshopApi.Data;
using Microsoft.AspNetCore.Mvc.NewtonsoftJson;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

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

app.UseStaticFiles();

app.UseAuthorization();

app.MapControllers();

app.Run();
