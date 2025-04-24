namespace FlightInfoService.Models;

public class Weather
{
    public required string AirportCode { get; set; }
    public required Airport Airport { get; set; }
    public required DateOnly Date { get; set; }
    public required double AverageTemp { get; set; }
    public required double MinTemp { get; set; }
    public required double MaxTemp { get; set; }
    public required double Precipitation { get; set; }
    public required double Snow { get; set; }
    public required double WindDirection { get; set; }
    public required double WindSpeed { get; set; }
    public required double Pressure { get; set; }

}
