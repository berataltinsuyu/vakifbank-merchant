using FluentValidation;
using FluentValidation.AspNetCore;
using Hangfire;
using Hangfire.SqlServer;
using Microsoft.EntityFrameworkCore;
using VbMerchant.Data;
using VbMerchant.Jobs;
using VbMerchant.Repositories;
using VbMerchant.Services;
using VbMerchant.Services.Concrete;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using VbMerchant.Services;


var builder = WebApplication.CreateBuilder(args);
var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("Jwt:Key yapılandırması eksik.");

if (Encoding.UTF8.GetByteCount(jwtKey) < 32)
{
    throw new InvalidOperationException("Jwt:Key en az 32 byte (256 bit) olmalıdır.");
}

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer           = true,
            ValidateAudience         = true,
            ValidateLifetime         = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer              = builder.Configuration["Jwt:Issuer"],
            ValidAudience            = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

// AuthService kaydı
builder.Services.AddScoped<AuthService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

builder.Services.AddFluentValidationAutoValidation(config =>
{
    config.DisableDataAnnotationsValidation = true;
});
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

builder.Services.AddHangfire(config =>
    config.UseSqlServerStorage(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddHangfireServer();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod());
});

builder.Services.AddScoped<IBasvuruRepository, BasvuruRepository>();
builder.Services.AddScoped<IBasvuruService, BasvuruService>();
builder.Services.AddScoped<VbMerchantJobService>();
builder.Services.AddHttpClient<DovizService>();
builder.Services.AddHttpClient<GeocodingService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "VbMerchant API v1");
        options.RoutePrefix = "swagger";
    });
}

using (var scope = app.Services.CreateScope())
{
    var jobService = scope.ServiceProvider.GetRequiredService<VbMerchantJobService>();
    jobService.ScheduleJobs();
}

app.UseHttpsRedirection();
app.UseAuthentication();  
app.UseAuthorization();
app.UseCors("AllowAngular");
app.UseStaticFiles();
app.UseHangfireDashboard("/hangfire");
app.MapControllers();

app.Run();
