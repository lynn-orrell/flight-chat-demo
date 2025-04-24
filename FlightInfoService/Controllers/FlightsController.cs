using FlightInfoService.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;

namespace FlightInfoService.Controllers;

public class FlightsController : ODataController
{
    private IEnumerable<Flight> _flights;

    public FlightsController(IEnumerable<Flight> flights)
    {
        _flights = flights;
    }

    [EnableQuery]
    [HttpGet]
    public ActionResult<IEnumerable<Flight>> Get()
    {
        return Ok(_flights);
    }

    [EnableQuery]
    [HttpGet("/odata/Flights({flightDate}, {tailNumber})")]
    public ActionResult<Flight> Get([FromRoute] DateOnly flightDate, [FromRoute] string tailNumber)
    {
        var flight = _flights.SingleOrDefault(f => f.FlightDate == flightDate && f.TailNumber == tailNumber);
        if (flight == null)
        {
            return NotFound();
        }

        return Ok(flight);
    }
}
