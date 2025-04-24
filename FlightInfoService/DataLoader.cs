using System.Collections.Concurrent;
using System.Globalization;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using FlightInfoService.Models;

namespace FlightInfoService;

public class DataLoader
{
    private static IEnumerable<Airport> _airports = Enumerable.Empty<Airport>();
    private static IEnumerable<Weather> _weather = Enumerable.Empty<Weather>();
    private static IEnumerable<Flight> _flights = Enumerable.Empty<Flight>();

    public DataLoader()
    {
        LoadAirports();
        LoadWeather();
        LoadFlights();
    }

    public IEnumerable<Airport> GetAirports()
    {
        return _airports;
    }

    public IEnumerable<Weather> GetWeather()
    {
        return _weather;
    }

    public IEnumerable<Flight> GetFlights()
    {
        return _flights;
    }

    private void LoadAirports()
    {
        using (var reader = new StreamReader("./DataFiles/airports_geolocation.csv"))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                csv.Context.RegisterClassMap<AirportMap>();
                _airports = new List<Airport>(csv.GetRecords<Airport>());
            }
    }

    private void LoadWeather()
    {
        using (var reader = new StreamReader("./DataFiles/weather_meteo_by_airport.csv"))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                csv.Context.RegisterClassMap<WeatherMap>();
                _weather = new List<Weather>(csv.GetRecords<Weather>());
            }
    }

    private void LoadFlights()
    {
        using (var reader = new StreamReader("./DataFiles/US_flights_2023.csv", Encoding.UTF8, true, 4096 * 20))
            using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                IgnoreBlankLines = true,
                HasHeaderRecord = true,
                BufferSize = 4096 * 20
            }))
            {
                csv.Context.RegisterClassMap<FlightMap>();
                _flights = new List<Flight>(csv.GetRecords<Flight>());
            }
    }

    class AirportMap : ClassMap<Airport>
    {
        public AirportMap()
        {
            Map(m => m.Code).Name("IATA_CODE");
            Map(m => m.Name).Name("AIRPORT");
            Map(m => m.City).Name("CITY");
            Map(m => m.State).Name("STATE");
            Map(m => m.Country).Name("COUNTRY");
            Map(m => m.Latitude).Name("LATITUDE");
            Map(m => m.Longitude).Name("LONGITUDE");
        }
    }

    class WeatherMap : ClassMap<Weather>
    {
        public WeatherMap()
        {
            Map(m => m.Date).Name("time");
            Map(m => m.AverageTemp).Name("tavg");
            Map(m => m.MinTemp).Name("tmin");
            Map(m => m.MaxTemp).Name("tmax");
            Map(m => m.Precipitation).Name("prcp");
            Map(m => m.Snow).Name("snow");
            Map(m => m.WindDirection).Name("wdir");
            Map(m => m.WindSpeed).Name("wspd");
            Map(m => m.Pressure).Name("pres");
            Map(m => m.AirportCode).Name("airport_id");
            // Lookup airport by code from _airports and assign to Airport property
            Map(m => m.Airport).Convert(row =>
            {
                var airportCode = row.Row.GetField<string>("airport_id");
                return DataLoader._airports.SingleOrDefault(a => a.Code == airportCode);
            });
        }
    }

    class FlightMap : ClassMap<Flight>
    {
        public FlightMap()
        {
            Map(m => m.FlightDate).Name("FlightDate");
            Map(m => m.DayOfWeek).Name("Day_Of_Week");
            Map(m => m.Airline).Name("Airline");
            Map(m => m.TailNumber).Name("Tail_Number");
            Map(m => m.DepartureAirportCode).Name("Dep_Airport");
            Map(m => m.DepartureAirport).Convert(row =>
            {
                var airportCode = row.Row.GetField<string>("Dep_Airport");
                return DataLoader._airports.SingleOrDefault(a => a.Code == airportCode);
            });
            Map(m => m.DepartureCity).Name("Dep_CityName");
            Map(m => m.DepartureTimeOfDay).Name("DepTime_label").TypeConverter<EnumConverter<DepartureTimeOfDay>>();
            Map(m => m.DepartureDelayMin).Name("Dep_Delay");
            Map(m => m.ArrivalAirportCode).Name("Arr_Airport");
            Map(m => m.ArrivalAirport).Convert(row =>
            {
                var airportCode = row.Row.GetField<string>("Arr_Airport");
                return DataLoader._airports.SingleOrDefault(a => a.Code == airportCode);
            });
            Map(m => m.ArrivalCity).Name("Arr_CityName");
            Map(m => m.ArrivalDelayMin).Name("Arr_Delay");
            Map(m => m.FlightDurationMin).Name("Flight_Duration");
            Map(m => m.DistanceType).Name("Distance_type").TypeConverter<DistanceTypeConverter>();
            Map(m => m.DelayCarrierMin).Name("Delay_Carrier");
            Map(m => m.DelayWeatherMin).Name("Delay_Weather");
            Map(m => m.DelayNASMin).Name("Delay_NAS");
            Map(m => m.DelaySecurityMin).Name("Delay_Security");
            Map(m => m.DelayLastAircraftMin).Name("Delay_LastAircraft");
            Map(m => m.Manufacturer).Name("Manufacturer");
            Map(m => m.Model).Name("Model");
            Map(m => m.AircraftAge).Name("Aicraft_age");
        }
    }

    class EnumConverter<T> : DefaultTypeConverter where T : struct
    {
        public override object ConvertFromString(string? text, IReaderRow row, MemberMapData memberMapData)
        {
            if (Enum.TryParse(typeof(T), text, true, out var result))
            {
                return result;
            }

            throw new InvalidOperationException($"Cannot convert '{text}' to {typeof(T)}");
        }
    }

    class DistanceTypeConverter : DefaultTypeConverter
    {
        public override object ConvertFromString(string? text, IReaderRow row, MemberMapData memberMapData)
        {
            if (text == "Short Haul >1500Mi")
            {
                return DistanceType.ShortHaul;
            }
            else if (text == "Medium Haul <3000Mi")
            {
                return DistanceType.MediumHaul;
            }
            else if (text == "Long Haul <6000Mi")
            {
                return DistanceType.LongHaul;
            }

            throw new InvalidOperationException($"Cannot convert '{text}' to DistanceType");
        }
    }
}
