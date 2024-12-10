using System.Net.Http.Headers;
using Alex.YouTube.Joker.Domain;
using Alex.YouTube.Joker.DomainServices;
using Alex.YouTube.Joker.DomainServices.Facades;
using Alex.YouTube.Joker.DomainServices.Options;
using Alex.YouTube.Joker.DomainServices.Services;
using Alex.YouTube.Joker.Host;
using Alex.YouTube.Joker.Host.Facades;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


var openAiApiKey = builder.Configuration["OpenAI:ApiKey"];

builder.Services.AddHttpClient<IGptFacade, GptFacade>(client =>
{
    client.BaseAddress = new Uri("https://api.openai.com/");
    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", openAiApiKey);
});
var s= AppDomain.CurrentDomain.BaseDirectory;
builder.Services.AddControllers();
builder.Services.AddScoped<IJokeService, JokeService>();
builder.Services.AddScoped<IVideoService, VideoService>();
builder.Services.AddScoped<IContentGenerator, JockerContentGenerator>();
builder.Services.AddScoped<IYouTubeFacade, YouTubeFacade>();
builder.Services.AddScoped<IImageService, ImageService>();
builder.Services.AddScoped<IChannelOptions, ChannelOptions>();

builder.Services.AddHostedService<ShortsGenerator>();

builder.Services.AddOptions<YouTube>()
    .Bind(builder.Configuration.GetSection("YouTube"))
    .ValidateDataAnnotations(); 

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();