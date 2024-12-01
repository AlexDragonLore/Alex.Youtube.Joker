using Alex.YouTube.Joker.Host.Controllers.Models;
using Alex.YouTube.Joker.Host.Facades;
using Microsoft.AspNetCore.Mvc;

namespace Alex.YouTube.Joker.Host.Controllers;

[ApiController]
[Route("api/v1/jokes")]
public class JokesController : ControllerBase
{
    private readonly IGptFacade _gptFacade;

    public JokesController(IGptFacade gptFacade)
    {
        _gptFacade = gptFacade;
    }

    
    [HttpPost]
    public async Task<ActionResult> CreateJoke([FromBody]CreateJokeRequest request, CancellationToken ct)
    {
        var joke = await _gptFacade.CreateJoke(request.Theme, ct);

        return Ok(joke);
    }
    
        
    [HttpPost("voice")]
    public async Task<ActionResult> ToVoice([FromBody]ToVoiceRequest request, CancellationToken ct)
    {
        var joke = await _gptFacade.ToVoice(request.Text, ct);

        return Ok(joke);
    }
}