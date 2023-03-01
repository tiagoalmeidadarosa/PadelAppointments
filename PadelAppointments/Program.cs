using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using PadelAppointments;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddDbContext<ApplicationDbContext>(
    options => options.UseSqlServer(builder.Configuration.GetConnectionString("Appointments")));

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "PadelAppointments API",
        Description = "Making appointments to your padel games",
        Version = "v1"
    });
});

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.WithOrigins(allowedOrigins);
        builder.WithExposedHeaders("Content-Disposition");
        builder.SetIsOriginAllowedToAllowWildcardSubdomains();
        builder.AllowAnyHeader();
        builder.AllowAnyMethod();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "PadelAppointments API V1");
    });
}

app.UseHttpsRedirection();
app.UseCors();

app.MapEndpoints();

app.Run();