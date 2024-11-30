using System;
using System.Threading;
using System.Threading.Tasks;
using Alex.YouTube.Joker.Domain;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Util.Store;
using Google.Apis.YouTube.v3;

namespace Alex.Youtube.Joker.Facades;

public class YouTubeFacade
{
    public YouTubeFacade()
    {
    }

    public Task UploadShort(YouTubeShort shorts, CancellationToken token)
    {
        throw new NotImplementedException();
    }

    private async Task Auth()
    {
        var credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
            new ClientSecrets
            {
                ClientId = "YOUR_CLIENT_ID",
                ClientSecret = "YOUR_CLIENT_SECRET",T
            },
            new[] { YouTubeService.Scope.Youtube },
            "user",
            CancellationToken.None,
            new FileDataStore(this.GetType().ToString())
        );
    }
}