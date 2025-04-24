namespace FlightInfoService.Models;

public class Flight
{
    public required DateOnly FlightDate { get; set; }
    public required int DayOfWeek { get; set; }
    public required string Airline { get; set; }
    public required string TailNumber { get; set; }
    public required string DepartureAirportCode { get; set; }
    public required Airport DepartureAirport { get; set; }
    public required string DepartureCity { get; set; }
    public required DepartureTimeOfDay DepartureTimeOfDay { get; set; }
    public required int DepartureDelayMin { get; set; }
    public required string ArrivalAirportCode { get; set; }
    public required Airport ArrivalAirport { get; set; }
    public required string ArrivalCity { get; set; }
    public required int ArrivalDelayMin { get; set; }
    public required int FlightDurationMin { get; set; }
    public required DistanceType DistanceType { get; set; }
    public required int DelayCarrierMin { get; set; }
    public required int DelayWeatherMin { get; set; }
    public required int DelayNASMin { get; set; }
    public required int DelaySecurityMin { get; set; }
    public required int DelayLastAircraftMin { get; set; }
    public required string Manufacturer { get; set; }
    public required string Model { get; set; }
    public required int AircraftAge { get; set; }
}
