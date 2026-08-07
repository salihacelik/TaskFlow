using Microsoft.EntityFrameworkCore;
using TaskFlow.API.Data;

var builder = WebApplication.CreateBuilder(args);

// 1. SERVİS TANIMLAMALARI 
builder.Services.AddDbContext<TaskFlowDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllers();

// SWAGGER (GÖRSEL ARAYÜZ) SERVİSLERİ 
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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
app.UseAuthorization();
app.MapControllers();

app.Run();