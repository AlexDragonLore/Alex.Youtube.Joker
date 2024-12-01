using Alex.YouTube.Joker.DomainServices.Facades;
using Alex.YouTube.Joker.DomainServices.Services;
using Alex.YouTube.Joker.Host.Controllers.Models;
using Alex.YouTube.Joker.Host.Facades;
using Microsoft.AspNetCore.Mvc;

namespace Alex.YouTube.Joker.Host.Controllers;

[ApiController]
[Route("api/v1/jokes")]
public class JokesController : ControllerBase
{
    private readonly IGptFacade _gptFacade;
    private readonly IJokeService _jokeService;

    public JokesController(IGptFacade gptFacade, IJokeService jokeService)
    {
        _gptFacade = gptFacade;
        _jokeService = jokeService;
    }

    
    [HttpPost]
    public async Task<ActionResult> CreateJoke([FromBody]CreateJokeRequest request, CancellationToken ct)
    {
        var joke = await _gptFacade.CreateJoke(request.Theme, ct);

        return Ok(joke);
    }    
    
    [HttpPost("for-shorts")]
    public async Task<ActionResult> GetJokesForShort([FromBody]CreateJokeRequest request, CancellationToken ct)
    {
        var joke = await _jokeService.GetJokesForShort(request.Theme, ct);

        return Ok(joke);
    }
    
        
    [HttpPost("voice")]
    public async Task<ActionResult> ToVoice([FromBody]ToVoiceRequest request, CancellationToken ct)
    {
        var joke = await _gptFacade.ToVoice(request.Text, ct);

        return Ok(joke);
    }
}