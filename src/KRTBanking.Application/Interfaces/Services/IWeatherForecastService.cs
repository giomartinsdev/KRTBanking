using KRTBanking.Domain.Context.WeatherForecastAggregate.Entities;

namespace KRTBanking.Application.Interfaces.Services;

public interface IWeatherForecastService
{
    public IEnumerable<WeatherForecast> GetWeatherForecast();
}