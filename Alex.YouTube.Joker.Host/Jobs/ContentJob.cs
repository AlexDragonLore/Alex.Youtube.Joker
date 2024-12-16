using Alex.YouTube.Joker.DomainServices.Facades;
using Alex.YouTube.Joker.DomainServices.Generators;
using Alex.YouTube.Joker.DomainServices.Options;

namespace Alex.YouTube.Joker.Host.Jobs;

public class ContentJob : IHostedService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<ContentJob> _logger;

    public ContentJob(IServiceScopeFactory serviceScopeFactory, ILogger<ContentJob> logger)
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

                //scope.ServiceProvider.GetService<IYouTubeFacade>()!.JustAuth2(scope.ServiceProvider.GetService<IChannelOptions>()!.GetChannel("Jocker")).Wait();
                foreach (var generator in scope.ServiceProvider.GetServices<IGenerator>())
                {
                    await generator.GenerateShorts(cancellationToken);
                }
                
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
            }

            var delay = TimeSpan.FromHours(24);
            _logger.LogInformation("[{now}]Sleep for minutes {minutes}", DateTime.UtcNow, delay.TotalMinutes);
            await Task.Delay(delay, cancellationToken);
        }

    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}