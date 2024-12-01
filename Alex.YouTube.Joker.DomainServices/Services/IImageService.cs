namespace Alex.YouTube.Joker.DomainServices.Services;

public interface IImageService
{
    string GetRandomImageWithText(string jokeText);
}