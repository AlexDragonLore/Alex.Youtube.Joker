using Alex.YouTube.Joker.Domain;
using Alex.YouTube.Joker.DomainServices.Services;
using Microsoft.AspNetCore.Mvc;

namespace Alex.YouTube.Joker.Host.Controllers;

[ApiController]
[Route("api/v1/video")]
public class VideoController : ControllerBase
{
    private readonly IVideoService _videoService;

    public VideoController(IVideoService videoService)
    {
        _videoService = videoService;
    }

    [HttpPost("from-audio")]
    public async Task<ActionResult> Create([FromBody]VideoRequest request, CancellationToken ct)
    {
        await _videoService.CreateVideoWithXabe(request, ct);

        return Ok();
    }
}