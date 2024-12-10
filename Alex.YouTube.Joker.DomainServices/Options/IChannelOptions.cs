using Alex.YouTube.Joker.Domain;

namespace Alex.YouTube.Joker.DomainServices.Options;

public interface IChannelOptions
{
    Channel GetChannel(string name);
}