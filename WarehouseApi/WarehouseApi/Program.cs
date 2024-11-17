using Microsoft.EntityFrameworkCore;
using WarehouseApi.Data_Access;
using WarehouseApi.Models;
using WarehouseApi.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddScoped<IProductService, ProductService>();//Inject the query service
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


//// Add InMemory Database Context
//WHILE TESTING, un-comment to use InMemory Database
builder.Services.AddDbContext<WarehouseContext>
    (opt => opt.UseInMemoryDatabase("Warehouse"));


//// Add MySQL Local Database Context

//WHILE TESTING, do not use true database
//builder.Services.AddDbContext<WarehouseContext>
//    (options => options.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add MySQL Local Database Context
//builder.Services.AddDbContext<WarehouseContext>(options => {
//    options.UseMySql(builder.Configuration.GetConnectionString("SimplyConnection"), new MySqlServerVersion(new Version(8,0,36)));
//});

// ConfigurationServices above
var app = builder.Build();
// Middleware from here and below

//// Initialize database from a file FOR TESTING ONLY!
//// WARNING, THIS DELETES THE EXISTING DATABASE TABLES! DO NOT USE UNLESS YOU WANT TO DELETE ALL DATA
///
//IDK if this is the best way of seeding a database
using (var scope = app.Services.CreateScope())
{
    
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<WarehouseContext>();
        context.Seed("MockTestingDatabase.csv");
    }
    catch(Exception e)
    {
        Console.WriteLine("Error While seeding context");
        Console.WriteLine(e.Message);
        return;
    }
}


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
