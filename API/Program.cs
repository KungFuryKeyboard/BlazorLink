using API.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using API;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(
                      builder => 
                      {
                          builder.WithOrigins("https://localhost:5001", "https://localhost:7100",
                                              "http://localhost:5000").AllowAnyMethod().AllowAnyHeader().AllowAnyOrigin();
                      });
});
var confbuilder = new ConfigurationBuilder();
confbuilder.AddJsonFile("appsettings.json");
IConfigurationRoot config = confbuilder.Build();
builder.Services.AddOptions<AppConfigOptions>().Bind(config.GetSection(AppConfigOptions.AppConfig));

builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseInMemoryDatabase(databaseName: "ShortURl"));
builder.Services.AddOptions<AppConfigOptions>().Bind(config.GetSection(AppConfigOptions.AppConfig));

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseCors();
app.UseHttpsRedirection();

app.MapGet("/{urlToken}", (HttpResponse request, string urlToken, [FromServices] ApplicationDbContext db) =>
{
    if (String.IsNullOrEmpty(urlToken))
        return Results.BadRequest("invalid token");
    if (urlToken.EndsWith("+"))
    {
        urlToken = urlToken.Remove(urlToken.Length - 1, 1);
        var b = db.ShortUrl.FirstOrDefault(x => x.Id == urlToken);

        if (b == null)
            return Results.NotFound();
        else
            return Results.Content(GetPreviewPage(b.DestinationURL), "text/html");
    }
    var x = db.ShortUrl.FirstOrDefault(x => x.Id == urlToken);
    if (x == null)
        return Results.BadRequest("url not found");

    // check if user saved the url with http / https and add one if they didn't.
    if (!x.DestinationURL.StartsWith("http://") && !x.DestinationURL.StartsWith("https://"))
        return Results.Redirect("http://" + x.DestinationURL);
    else
        return Results.Redirect(x.DestinationURL);
})
.WithName("GetURL")
.WithOpenApi();


app.MapPost("", ([FromBody] string urlToShorten, [FromServices] ApplicationDbContext db, [FromServices] IOptions<AppConfigOptions> _config) =>
{
    if (!String.IsNullOrEmpty(urlToShorten))
    {
        var newUrl = new ShortUrl();
        newUrl.DestinationURL = urlToShorten.Trim();
        Console.WriteLine("URL to shorten: " + urlToShorten);
        string UrlToken = generateToken(Length: 7);
        newUrl.Id = UrlToken;
        db.ShortUrl.Add(newUrl);
        db.SaveChanges();
        string ReturnUrl = _config.Value.DefaultDomain + "/" + UrlToken;
        Console.WriteLine(ReturnUrl);
        return Results.Ok(ReturnUrl);
    }
    else
    {
        return Results.BadRequest("invalid url");
    }
})
.WithName("PostURL")
.WithOpenApi();

string GetPreviewPage(string url)
{

    var page = $@"<html><head><link rel='stylesheet' href='https://stackpath.bootstrapcdn.com/bootswatch/4.5.2/sketchy/bootstrap.min.css'></head><body>
      <div class='container'>
          <div class='row' style='height:100%'>

            <div class='col-md-12 d-flex flex-wrap align-items-center text-center'>
            <a href='{url}'><span class='btn btn-dark'>  <h3 center>Destination : {url}</h1></span></a>
            </div>
          </div>
      </div>
      </body></html>
      ";

    return page;

}



string generateToken(int Length)
{
    Guid id = Guid.NewGuid();
    Console.WriteLine($"Guid : {id}");
    var UrlToken = Convert.ToBase64String(id.ToByteArray())
        .Replace("_", "")
        .Replace("/", "")
        .Replace("+", "")
        .Replace("-", "")
        .Substring(0, Length);
    Console.WriteLine($"base 64 : {UrlToken}");

    Console.WriteLine($"Shuffle base 64 : {UrlToken}");
    return UrlToken;
}

app.Run();

