using Alex.YouTube.Joker.Domain;
using Alex.YouTube.Joker.DomainServices;
using Alex.YouTube.Joker.DomainServices.Facades;
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
    private readonly IYouTubeFacade _youTubeFacade;

    public VideoController(IVideoService videoService, IContentGenerator contentGenerator, IYouTubeFacade youTubeFacade)
    {
        _videoService = videoService;
        _contentGenerator = contentGenerator;
        _youTubeFacade = youTubeFacade;
    }

    [HttpPost("from-audio")]
    public async Task<ActionResult> Create([FromBody]VideoRequest request, CancellationToken ct)
    {
        await _videoService.CreateVideoWithXabe(request, ct);

        return Ok();
    }
    
    [HttpPost("generate-shorts")]
    public async Task<ActionResult> CreateJoke([FromBody]CreateJokeRequest request, CancellationToken ct)
    {
        await _contentGenerator.GenerateShorts(request.Theme, ct);
        
        return Ok();
    }     
    
    [HttpPost("upload-short")]
    public async Task<ActionResult> UploadShort([FromBody]YouTubeShort request, CancellationToken ct)
    {
        await _youTubeFacade.UploadShort(request, ct);

        return Ok();
    }  
}