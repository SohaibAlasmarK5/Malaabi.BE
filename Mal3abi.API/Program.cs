using Mal3abi.Core.Entites;
using Mal3abi.Core.Extensions;
using Mal3abi.Core.Services;
using Mal3abi.Infra.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Scalar.AspNetCore;
using System;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.AddDbContext<MalaabiDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
    ));

builder.Services.AddScopeInjectableServices(typeof(AuthService).Assembly);

builder.Services.AddIdentity<User, IdentityRole<Guid>>()
    .AddEntityFrameworkStores<MalaabiDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddControllers();

var app = builder.Build();

await app.Services.SeedRolesAndSuperAdminAsync();

app.MapControllers();

if (app.Environment.IsDevelopment())
{
    // Serves the raw OpenAPI 3.1 JSON document
    app.MapOpenApi();

    // 1. Scalar UI (available at /scalar/v1)
    app.MapScalarApiReference(options =>
    {
        options
            .WithTitle("Malaabi API")
            .WithTheme(ScalarTheme.Moon)
            .ForceDarkMode();
    });

    // 2. Classic Swagger UI (available at /swagger)
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "Malaabi API v1");
        options.RoutePrefix = "swagger"; // Sets the URL path to /swagger
    });
}

app.MapGet("/", () => "Hello World!");

app.Run();