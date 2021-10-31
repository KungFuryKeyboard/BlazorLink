using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Model;
using API.Domain;
using Microsoft.Extensions.Options;

namespace API.Controllers
{
  [ApiController]
  [Route("")]
  public class AppController : ControllerBase
  {

    private readonly ILogger<AppController> _logger;
    private readonly ApplicationDbContext _db;
    private readonly IOptions<AppConfigOptions> _config;

    public AppController(ILogger<AppController> logger, ApplicationDbContext db, IOptions<AppConfigOptions> config)
    {
      _logger = logger;
      _db = db;
      _config = config;
    }




    [HttpPost]
    public ActionResult<string> Post([FromBody] string urlToShorten)
    {
      if (!String.IsNullOrEmpty(urlToShorten))
      {
        var newUrl = new ShortUrl();
        newUrl.DestinationURL = urlToShorten.Trim();
        Console.WriteLine("URL to shorten: " + urlToShorten);
        string UrlToken = generateToken(Length: 7);
        newUrl.Id = UrlToken;
        _db.ShortUrl.Add(newUrl);
        _db.SaveChanges();
        string ReturnUrl = _config.Value.DefaultDomain + "/" + UrlToken;
        Console.WriteLine(ReturnUrl);
       

                return ReturnUrl;
      }
      else
      {
        return BadRequest("invalid url");
      }

    }

    [HttpGet]
    [Route("/{urlToken}")]
    public ActionResult AppRedirect([FromRoute] string urlToken)
    {


      if (String.IsNullOrEmpty(urlToken))
        return BadRequest("invalid token");

      if (urlToken.EndsWith("+"))
      {
        urlToken = urlToken.Remove(urlToken.Length - 1, 1);
        var b = _db.ShortUrl.FirstOrDefault(x => x.Id == urlToken);
        
        if (b == null)
          return BadRequest("url not found");
        else
          return Content(GetPreviewPage(b.DestinationURL),"text/html");
      }

      var x = _db.ShortUrl.FirstOrDefault(x => x.Id == urlToken);

      if (x == null)
        return BadRequest("url not found");

      // check if user saved the url with http / https and add one if they didn't.
      if (!x.DestinationURL.StartsWith("http://") && !x.DestinationURL.StartsWith("https://"))
        return new RedirectResult("http://" + x.DestinationURL);
      else
        return new RedirectResult(x.DestinationURL);

      //RedirectResult redirect = new RedirectResult(x.DestinationURL);
      //Console.Write(redirect.Url);

      //return redirect;
    }

    private string GetPreviewPage(string url){

      var page =$@"<html><head><link rel='stylesheet' href='https://stackpath.bootstrapcdn.com/bootswatch/4.5.2/sketchy/bootstrap.min.css'></head><body>
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



    private string generateToken(int Length)
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


    // [HttpGet]
    // public IEnumerable<WeatherForecast> Get()
    // {
    //     var rng = new Random();
    //     return Enumerable.Range(1, 5).Select(index => new WeatherForecast
    //     {
    //         Date = DateTime.Now.AddDays(index),
    //         TemperatureC = rng.Next(-20, 55),
    //         Summary = Summaries[rng.Next(Summaries.Length)]
    //     })
    //     .ToArray();
    // }
  }
}
