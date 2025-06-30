using KRTBanking.Domain.Context.WeatherForecastAggregate.Entities;
using KRTBanking.Application.Interfaces.Services;

namespace KRTBanking.Application.Services;

public class WeatherForecastService :  IWeatherForecastService
{
    public IEnumerable<WeatherForecast> GetWeatherForecast()
    {
        string[] summaries = GetSummaries();
        return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = summaries[Random.Shared.Next(summaries.Length)]
            })
            .ToArray();
    }

    private static string[] GetSummaries()
    {
        return [
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        ];
    }


}