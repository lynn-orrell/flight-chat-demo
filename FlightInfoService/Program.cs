using FlightInfoService;
using FlightInfoService.Models;
using Microsoft.AspNetCore.OData;
using Microsoft.OData.ModelBuilder;

var builder = WebApplication.CreateBuilder(args);

var modelBuilder = new ODataConventionModelBuilder();
modelBuilder.EntitySet<Airport>("Airports").EntityType.HasKey(a => a.Code);
modelBuilder.EntitySet<Airport>("Airports").EntityType.Property(p => p.Code).HasDescription().HasDescription("IATA code of the airport");
modelBuilder.EntitySet<Airport>("Airports").EntityType.Property(p => p.Name).HasDescription().HasDescription("Name of the airport");
modelBuilder.EntitySet<Airport>("Airports").EntityType.Property(p => p.City).HasDescription().HasDescription("City where the airport is located");
modelBuilder.EntitySet<Airport>("Airports").EntityType.Property(p => p.Latitude).HasDescription().HasDescription("Latitude of the airport location");
modelBuilder.EntitySet<Airport>("Airports").EntityType.Property(p => p.Longitude).HasDescription().HasDescription("Longitude of the airport location");
modelBuilder.EntitySet<Airport>("Airports").EntityType.Property(p => p.State).HasDescription().HasDescription("State where the airport is located");
modelBuilder.EntitySet<Airport>("Airports").EntityType.Property(p => p.Country).HasDescription().HasDescription("Country where the airport is located");

modelBuilder.EntitySet<Flight>("Flights").EntityType.HasKey(f => f.FlightDate).HasKey(f => f.TailNumber);
modelBuilder.EntitySet<Flight>("Flights").EntityType.Property(p => p.FlightDate).HasDescription().HasDescription("The date of the flight");
modelBuilder.EntitySet<Flight>("Flights").EntityType.Property(p => p.DayOfWeek).HasDescription().HasDescription("Numeric representation of the day of the week (1=Monday, 7=Sunday)");
modelBuilder.EntitySet<Flight>("Flights").EntityType.Property(p => p.Airline).HasDescription().HasDescription("The airline operating the flight");
modelBuilder.EntitySet<Flight>("Flights").EntityType.Property(p => p.TailNumber).HasDescription().HasDescription("The aircraft's tail number");
modelBuilder.EntitySet<Flight>("Flights").EntityType.Property(p => p.DepartureAirportCode).HasDescription().HasDescription("IATA code of the departure airport");
modelBuilder.EntitySet<Flight>("Flights").EntityType.Property(p => p.DepartureCity).HasDescription().HasDescription("City from which the flight departs");
modelBuilder.EntitySet<Flight>("Flights").EntityType.EnumProperty(p => p.DepartureTimeOfDay).HasDescription().HasDescription("Time of day category for departure (e.g., Morning, Afternoon, Evening)");
modelBuilder.EntitySet<Flight>("Flights").EntityType.Property(p => p.DepartureDelayMin).HasDescription().HasDescription("Departure delay in minutes");
modelBuilder.EntitySet<Flight>("Flights").EntityType.Property(p => p.ArrivalAirportCode).HasDescription().HasDescription("IATA code of the arrival airport");
modelBuilder.EntitySet<Flight>("Flights").EntityType.Property(p => p.ArrivalCity).HasDescription().HasDescription("City where the flight arrives");
modelBuilder.EntitySet<Flight>("Flights").EntityType.Property(p => p.ArrivalDelayMin).HasDescription().HasDescription("Arrival delay in minutes");
modelBuilder.EntitySet<Flight>("Flights").EntityType.Property(p => p.FlightDurationMin).HasDescription().HasDescription("Total duration of the flight in minutes");
modelBuilder.EntitySet<Flight>("Flights").EntityType.EnumProperty(p => p.DistanceType).HasDescription().HasDescription("Category of flight distance (e.g., Short-haul, Medium-haul, Long-haul)");
modelBuilder.EntitySet<Flight>("Flights").EntityType.Property(p => p.DelayCarrierMin).HasDescription().HasDescription("Delay in minutes caused by the carrier");
modelBuilder.EntitySet<Flight>("Flights").EntityType.Property(p => p.DelayWeatherMin).HasDescription().HasDescription("Delay in minutes caused by weather conditions");
modelBuilder.EntitySet<Flight>("Flights").EntityType.Property(p => p.DelayNASMin).HasDescription().HasDescription("Delay in minutes caused by National Airspace System (NAS) issues");
modelBuilder.EntitySet<Flight>("Flights").EntityType.Property(p => p.DelaySecurityMin).HasDescription().HasDescription("Delay in minutes caused by security issues");
modelBuilder.EntitySet<Flight>("Flights").EntityType.Property(p => p.DelayLastAircraftMin).HasDescription().HasDescription("Delay in minutes caused by late arrival of the previous aircraft");
modelBuilder.EntitySet<Flight>("Flights").EntityType.Property(p => p.Manufacturer).HasDescription().HasDescription("Aircraft manufacturer's name");
modelBuilder.EntitySet<Flight>("Flights").EntityType.Property(p => p.Model).HasDescription().HasDescription("Aircraft model designation");
modelBuilder.EntitySet<Flight>("Flights").EntityType.Property(p => p.AircraftAge).HasDescription().HasDescription("Age of the aircraft in years");
modelBuilder.EntitySet<Flight>("Flights").EntityType.HasRequired(f => f.DepartureAirport, (f, a) => f.DepartureAirportCode == a.Code);
modelBuilder.EntitySet<Flight>("Flights").EntityType.HasRequired(f => f.ArrivalAirport, (f, a) => f.ArrivalAirportCode == a.Code);

modelBuilder.EntitySet<Weather>("Weather").EntityType.HasKey(w => w.AirportCode).HasKey(w => w.Date);
modelBuilder.EntitySet<Weather>("Weather").EntityType.Property(p => p.Date).HasDescription().HasDescription("The date of the weather observation");
modelBuilder.EntitySet<Weather>("Weather").EntityType.Property(p => p.AirportCode).HasDescription().HasDescription("IATA code of the airport");
modelBuilder.EntitySet<Weather>("Weather").EntityType.Property(p => p.AverageTemp).HasDescription().HasDescription("Average temperature in degrees Celsius");
modelBuilder.EntitySet<Weather>("Weather").EntityType.Property(p => p.MinTemp).HasDescription().HasDescription("Minimum temperature in degrees Celsius");
modelBuilder.EntitySet<Weather>("Weather").EntityType.Property(p => p.MaxTemp).HasDescription().HasDescription("Maximum temperature in degrees Celsius");
modelBuilder.EntitySet<Weather>("Weather").EntityType.Property(p => p.Precipitation).HasDescription().HasDescription("Precipitation amount in millimeters");
modelBuilder.EntitySet<Weather>("Weather").EntityType.Property(p => p.Snow).HasDescription().HasDescription("Snow amount in millimeters");
modelBuilder.EntitySet<Weather>("Weather").EntityType.Property(p => p.WindDirection).HasDescription().HasDescription("Wind direction in degrees");
modelBuilder.EntitySet<Weather>("Weather").EntityType.Property(p => p.WindSpeed).HasDescription().HasDescription("Wind speed in knots");
modelBuilder.EntitySet<Weather>("Weather").EntityType.Property(p => p.Pressure).HasDescription().HasDescription("Atmospheric pressure in hPa");
modelBuilder.EntitySet<Weather>("Weather").EntityType.HasRequired(w => w.Airport, (w, a) => w.AirportCode == a.Code);


builder.Services.AddControllers().AddOData(
    options => options.Select().Expand().Filter().OrderBy().SetMaxTop(null).Count()
                      .AddRouteComponents("odata", modelBuilder.GetEdmModel())
);

DataLoader dataLoader = new DataLoader();
builder.Services.AddSingleton(dataLoader.GetAirports());
builder.Services.AddSingleton(dataLoader.GetWeather());
builder.Services.AddSingleton(dataLoader.GetFlights());

// builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
// builder.Services.AddEndpointsApiExplorer();
// builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
// if (app.Environment.IsDevelopment())
// {
//     app.UseSwagger();
//     app.UseSwaggerUI();
// }

// app.UseHttpsRedirection();

// app.UseAuthorization();

app.MapControllers();

app.UseODataRouteDebug();

app.Run();
