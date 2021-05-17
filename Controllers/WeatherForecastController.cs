using EasyCaching.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HybridCachingWithEasyCaching.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;
        private readonly IHybridCachingProvider provider;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, IHybridCachingProvider provider)
        {
            _logger = logger;
            this.provider = provider;
        }

        [HttpGet]
        public async Task<IEnumerable<WeatherForecast>> Get()
        {
            var rng = new Random();
            
            var cachedValue = await provider.GetAsync<WeatherForecast[]>("weather-list");
            if (cachedValue.HasValue)
            {
                _logger.LogInformation($"cached-value: {cachedValue.Value}");
                return cachedValue.Value;
            }

            var result = Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            })
            .ToArray();
            provider.Set("weather-list", result, TimeSpan.FromSeconds(10));

            return result;
        }
    }
}
