using FluentValidation;
using FluentValidation.AspNetCore;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.EntityFrameworkCore;
using VbMerchant.Data;
using VbMerchant.Jobs;
using VbMerchant.Repositories;
using VbMerchant.Services;
using VbMerchant.Services.Concrete;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;



var builder = WebApplication.CreateBuilder(args);
var defaultConnection = PostgresConnectionString.Normalize(
    builder.Configuration.GetConnectionString("DefaultConnection"));
var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("Jwt:Key yapılandırması eksik.");

if (Encoding.UTF8.GetByteCount(jwtKey) < 32)
{
    throw new InvalidOperationException("Jwt:Key en az 32 byte (256 bit) olmalıdır.");
}

builder.Configuration
    .AddJsonFile("appsettings.json",optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(defaultConnection));


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

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<AuthSeedService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

builder.Services.AddFluentValidationAutoValidation(config =>
{
    config.DisableDataAnnotationsValidation = true;
});
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// builder.Services.AddHangfire(config =>
//     config.UsePostgreSqlStorage(c =>
//         c.UseNpgsqlConnection(
//             builder.Configuration.GetConnectionString("DefaultConnection"))));

// builder.Services.AddHangfireServer();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod());
});

builder.Services.AddScoped<IBasvuruRepository, BasvuruRepository>();
builder.Services.AddScoped<IBasvuruService, BasvuruService>();
// builder.Services.AddScoped<VbMerchantJobService>();
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

// using (var scope = app.Services.CreateScope())
// {
//     var jobService = scope.ServiceProvider.GetRequiredService<VbMerchantJobService>();
//     jobService.ScheduleJobs();
// }

using (var scope = app.Services.CreateScope())
{
    var authSeedService = scope.ServiceProvider.GetRequiredService<AuthSeedService>();
    await authSeedService.SeedAsync();
}

app.UseHttpsRedirection();
app.UseAuthentication();  
app.UseAuthorization();
app.UseCors("AllowAngular");
app.UseStaticFiles();
// app.UseHangfireDashboard();
app.MapControllers();

app.Run();
