using KRTBanking.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace KRTBanking.API.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    private readonly IWeatherForecastService _weatherForecastService;
    public WeatherForecastController(IWeatherForecastService  weatherForecastService)
    {
        _weatherForecastService = weatherForecastService;
    }

    [HttpGet("")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public ActionResult LoadSummary()
    {
        return Ok(_weatherForecastService.GetWeatherForecast());
    }
}