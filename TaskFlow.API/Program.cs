using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using TaskFlow.API.Data;

var builder = WebApplication.CreateBuilder(args);

// 1. SERVİS TANIMLAMALARI 
builder.Services.AddDbContext<TaskFlowDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllers();

// JWT KİMLİK DOĞRULAMA SERVİSİ (YENİ EKLENEN KISIM)
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "TaskFlowIssuer",
            ValidAudience = builder.Configuration["Jwt:Audience"] ?? "TaskFlowAudience",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? "super_secret_taskflow_key_123456789"))
        };
    });

// SWAGGER (GÖRSEL ARAYÜZ) SERVİSLERİ VE KİLİT BUTONU EKLENTİSİ (YENİ EKLENEN KISIM)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Token'ı buraya girin. Format: Bearer {token}",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular",
        policy => policy.WithOrigins("http://localhost:4200") // Angular'ın adresi
                        .AllowAnyHeader()
                        .AllowAnyMethod());
});
// 2. PROJENİN İNŞA EDİLMESİ
var app = builder.Build(); 

// SWAGGER KULLANIMI AKTİF EDİLDİ
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// 3. MİDDLEWARE TANIMLAMALARI
app.UseHttpsRedirection();

// KİMLİK DOĞRULAMA MİDDLEWARE'İ (YENİ EKLENEN KISIM - Mutlaka UseAuthorization'dan önce olmalı)
app.UseAuthentication(); 
app.UseCors("AllowAngular"); // Sıralama çok önemli, UseAuthorization'dan üstte olmalı!
app.UseAuthorization();

app.MapControllers();

app.Run();