using Alex.YouTube.Joker.Domain;
using Alex.YouTube.Joker.DomainServices;
using Alex.YouTube.Joker.DomainServices.Facades;
using Alex.YouTube.Joker.DomainServices.Options;

namespace Alex.YouTube.Joker.Host;

public class ShortsGenerator : IHostedService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<ShortsGenerator> _logger;

    public ShortsGenerator(IServiceScopeFactory serviceScopeFactory, ILogger<ShortsGenerator> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceScopeFactory.CreateScope();
                
                await scope.ServiceProvider.GetRequiredService<IContentGenerator>()
                    .GenerateShorts(Themes.All[Random.Shared.Next(0, Themes.All.Count - 1)], cancellationToken);
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
            }

            var delay = TimeSpan.FromHours(3).Add(TimeSpan.FromMinutes(Random.Shared.NextInt64(0, 120)));
            _logger.LogInformation("[{now}]Sleep for minutes {minutes}", DateTime.UtcNow, delay.TotalMinutes);
            await Task.Delay(delay, cancellationToken);
        }
        
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}