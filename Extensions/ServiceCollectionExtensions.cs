using Microsoft.Extensions.DependencyInjection;
using TollFeeCalculator.Services;

namespace TollFeeCalculator.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers all toll calculator services with the dependency injection container
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="apiTimeout">Optional timeout for holiday API calls (default: 5 seconds)</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection Initialize(this IServiceCollection services, TimeSpan? apiTimeout = null)
    {
        services.AddHttpClient<IHolidayService, HolidayService>(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        // Register holiday service with custom timeout
        services.AddSingleton<IHolidayService>(provider =>
        {
            var httpClientFactory = provider.GetRequiredService<IHttpClientFactory>();
            var httpClient = httpClientFactory.CreateClient(nameof(HolidayService));
            return new HolidayService(httpClient, apiTimeout);
        });

        // Register toll calculator
        services.AddScoped<ITollCalculator, TollCalculator>();

        return services;
    }
}