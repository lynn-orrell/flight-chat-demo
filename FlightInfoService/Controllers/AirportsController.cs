using FlightInfoService.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;

namespace FlightInfoService.Controllers;

public class AirportsController : ODataController
{
    private IEnumerable<Airport> _airports;

    public AirportsController(IEnumerable<Airport> airports)
    {
        _airports = airports;
    }

    [EnableQuery]
    [HttpGet]
    public ActionResult<IEnumerable<Airport>> Get()
    {
        return Ok(_airports);
    }

    [EnableQuery]
    public ActionResult<Airport> Get([FromRoute] string key)
    {
        var airport = _airports.SingleOrDefault(a => a.Code == key);
        if (airport == null)
        {
            return NotFound();
        }

        return Ok(airport);
    }
}
