using ECommerce.Domin.Contracts;
using ECommerce.Persistence.Repositoris;
using StackExchange.Redis;

namespace E_CommereceAPI.Extention
{
    public static class RedisExtension
    {
        public static IServiceCollection AddRedisBasket(
            this IServiceCollection services,
            IConfiguration configuration,
            IHostEnvironment environment)
        {
            var useInMemory = configuration.GetValue("Basket:UseInMemory", environment.IsDevelopment());

            if (useInMemory)
            {
                services.AddSingleton<InMemoryBasketRepository>();
                services.AddScoped<IBasketRepository>(sp => sp.GetRequiredService<InMemoryBasketRepository>());
                return services;
            }

            var redisConnection = configuration.GetConnectionString("Redis")
                ?? throw new InvalidOperationException("Redis connection string 'ConnectionStrings:Redis' is not configured.");

            services.AddSingleton<IConnectionMultiplexer>(_ =>
            {
                var options = ConfigurationOptions.Parse(redisConnection);
                options.AbortOnConnectFail = false;
                return ConnectionMultiplexer.Connect(options);
            });
            services.AddScoped<IBasketRepository, RedisBasketRepository>();

            return services;
        }
    }
}
