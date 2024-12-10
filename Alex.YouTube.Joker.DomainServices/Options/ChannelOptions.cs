using Alex.YouTube.Joker.Domain;
using Microsoft.Extensions.Options;

namespace Alex.YouTube.Joker.DomainServices.Options;

public class ChannelOptions : IChannelOptions
{
    private readonly Domain.YouTube _options;

    public ChannelOptions(IOptions<Domain.YouTube> options)
    {
        _options = options.Value;
    }

    public Channel GetChannel(string name)
    {
        return _options.Channels.Single(c => c.Name == name);
    }
}