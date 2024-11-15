using Microsoft.EntityFrameworkCore;
using WarehouseApi.Data_Access;
using WarehouseApi.Models;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//// Add InMemory Database Context
//builder.Services.AddDbContext<WarehouseContext>
//    (opt => opt.UseInMemoryDatabase("Warehouse"));

//// Add MySQL Local Database Context
//builder.Services.AddDbContext<WarehouseContext>
//    (options => options.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add MySQL Local Database Context
builder.Services.AddDbContext<WarehouseContext>(options => {
    options.UseMySql(builder.Configuration.GetConnectionString("SimplyConnection"), new MySqlServerVersion(new Version(8,0,36)));
});

// ConfigurationServices above
var app = builder.Build();
// Middleware from here and below


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
