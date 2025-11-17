using LuckyFridayCalculator.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Support UTF-8 and Vietnamese characters
        options.JsonSerializerOptions.Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
        options.JsonSerializerOptions.WriteIndented = false;
    });

builder.Services.AddDbContext<LuckyFridayDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.MapControllers();

app.Run();
