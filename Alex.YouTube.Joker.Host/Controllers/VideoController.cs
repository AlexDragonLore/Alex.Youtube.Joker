using Alex.YouTube.Joker.Domain;
using Alex.YouTube.Joker.DomainServices;
using Alex.YouTube.Joker.DomainServices.Facades;
using Alex.YouTube.Joker.DomainServices.Generators;
using Alex.YouTube.Joker.DomainServices.Services;
using Alex.YouTube.Joker.Host.Controllers.Models;
using Microsoft.AspNetCore.Mvc;

namespace Alex.YouTube.Joker.Host.Controllers;

[ApiController]
[Route("api/v1/video")]
public class VideoController : ControllerBase
{
    private readonly IContentGenerator _contentGenerator;

    public VideoController(IContentGenerator contentGenerator)
    {
        _contentGenerator = contentGenerator;
    }

    [HttpPost("generate-shorts")]
    public async Task<ActionResult> CreateJoke([FromBody] CreateJokeRequest request, CancellationToken ct)
    {
        await _contentGenerator.GenerateShorts(request.Theme, ct);

        return Ok();
    }
}