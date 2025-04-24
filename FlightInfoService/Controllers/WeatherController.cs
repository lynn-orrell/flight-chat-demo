using FlightInfoService.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;

namespace FlightInfoService.Controllers;

public class WeatherController : ODataController
{
    private IEnumerable<Weather> _weather;

    public WeatherController(IEnumerable<Weather> weather)
    {
        _weather = weather;
    }

    [EnableQuery]
    [HttpGet]
    public ActionResult<IEnumerable<Weather>> Get()
    {
        return Ok(_weather);
    }

    [EnableQuery]
    [HttpGet("/odata/Weather({airportCode}, {date})")]
    public ActionResult<Weather> Get([FromRoute] string airportCode, [FromRoute] DateOnly date)
    {
        var airport = _weather.SingleOrDefault(a => a.AirportCode == airportCode && a.Date == date);
        if (airport == null)
        {
            return NotFound();
        }

        return Ok(airport);
    }
}
