using Alex.YouTube.Joker.Domain;
using Alex.YouTube.Joker.DomainServices;
using Alex.YouTube.Joker.DomainServices.Services;
using Alex.YouTube.Joker.Host.Controllers.Models;
using Microsoft.AspNetCore.Mvc;

namespace Alex.YouTube.Joker.Host.Controllers;

[ApiController]
[Route("api/v1/video")]
public class VideoController : ControllerBase
{
    private readonly IVideoService _videoService;
    private readonly IContentGenerator _contentGenerator;

    public VideoController(IVideoService videoService, IContentGenerator contentGenerator)
    {
        _videoService = videoService;
        _contentGenerator = contentGenerator;
    }

    [HttpPost("from-audio")]
    public async Task<ActionResult> Create([FromBody]VideoRequest request, CancellationToken ct)
    {
        await _videoService.CreateVideoWithXabe(request, ct);

        return Ok();
    }
    
    [HttpPost]
    public async Task<ActionResult> CreateJoke([FromBody]CreateJokeRequest request, CancellationToken ct)
    {
        var joke = await _contentGenerator.GetShort(request.Theme, ct);

        return Ok(joke);
    }  
}