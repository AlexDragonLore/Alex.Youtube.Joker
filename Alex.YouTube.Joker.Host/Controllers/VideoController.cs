using Alex.YouTube.Joker.DomainServices.Generators;
using Alex.YouTube.Joker.Host.Controllers.Models;
using Alex.YouTube.Joker.Host.Facades;
using Microsoft.AspNetCore.Mvc;

namespace Alex.YouTube.Joker.Host.Controllers;

[ApiController]
[Route("api/v1/video")]
public class VideoController : ControllerBase
{
    private readonly IContentGenerator _contentGenerator;
    private readonly IServiceProvider _serviceProvider;

    public VideoController(IContentGenerator contentGenerator, IServiceProvider serviceProvider)
    {
        _contentGenerator = contentGenerator;
        _serviceProvider = serviceProvider;
    }

    [HttpPost("generate-shorts")]
    public async Task<ActionResult> CreateJoke([FromBody] CreateJokeRequest request, CancellationToken ct)
    {
        await _contentGenerator.GenerateShorts(request.Theme, ct);

        return Ok();
    }
    
    
    [HttpPost("test")]
    public async Task<ActionResult> Test([FromBody] CreateJokeRequest request, CancellationToken ct)
    {
        await _serviceProvider.GetRequiredService<IYandexFacade>().ToVoice(request.Theme, ct);

        return Ok();
    }
}