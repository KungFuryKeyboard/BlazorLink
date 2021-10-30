using API;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);



// Add services to the container.
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(
                      builder =>
                      {
                          builder.WithOrigins("https://localhost:5001",
                                              "http://localhost:5000").AllowAnyMethod().AllowAnyHeader();
                      });
});


builder.Services.AddControllers();

builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseInMemoryDatabase(databaseName: "ShortURl"));
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


var confbuilder = new ConfigurationBuilder();
confbuilder.AddJsonFile("appsettings.json");
IConfigurationRoot config = confbuilder.Build();


builder.Services.AddOptions<AppConfigOptions>().Bind(config.GetSection(AppConfigOptions.AppConfig));

var app = builder.Build();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}



app.UseHttpsRedirection();

app.UseCors();

app.UseAuthorization();

app.MapControllers();


app.Run();
