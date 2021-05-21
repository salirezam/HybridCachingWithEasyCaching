using EasyCaching.Core;
using EasyCaching.Redis;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
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
        private readonly IRedisDatabaseProvider redisDatabaseProvider;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, IHybridCachingProvider provider, IRedisDatabaseProvider redisDatabaseProvider)
        {
            _logger = logger;
            this.provider = provider;
            this.redisDatabaseProvider = redisDatabaseProvider;
        }

        [HttpGet]
        public async Task<IEnumerable<WeatherForecast>> Get()
        {
            var rng = new Random();
            
            var cachedValue = await provider.GetAsync<WeatherForecast[]>("weather-list");
            if (cachedValue.HasValue)
            {
                foreach (var value in cachedValue.Value)
                    _logger.LogInformation($"cached-value: {value}");

                return cachedValue.Value;
            }

            var result = Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            })
            .ToArray();
            try
            {
                if (redisDatabaseProvider.GetDatabase().IsConnected(default(RedisKey))) // To check Redis connectivity
                    await provider.SetAsync("weather-list", result, TimeSpan.FromSeconds(10));
            }catch(RedisConnectionException ex)
            {
                _logger.LogError($"Redis is down. Error: {ex.Message}");
            }

            return result;
        }
    }
}
