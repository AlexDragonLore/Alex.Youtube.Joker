using System.Net.Http.Headers;
using Alex.YouTube.Joker.Domain;
using Alex.YouTube.Joker.DomainServices.Facades;
using Alex.YouTube.Joker.DomainServices.Generators;
using Alex.YouTube.Joker.DomainServices.Options;
using Alex.YouTube.Joker.DomainServices.Services;
using Alex.YouTube.Joker.Host.Facades;
using Alex.YouTube.Joker.Host.Jobs;
using Google.Apis.YouTube.v3;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


var openAiApiKey = builder.Configuration["OpenAI:ApiKey"];

builder.Services.AddHttpClient<IGptFacade, GptFacade>(client =>
{
    client.BaseAddress = new Uri("https://api.openai.com/");
    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", openAiApiKey);
});

var yandexKey = builder.Configuration["Yandex:IamToken"];

builder.Services.AddHttpClient<IYandexFacade, YandexFacade>(client =>
{
    client.BaseAddress = new Uri("https://tts.api.cloud.yandex.net/");
    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", yandexKey);
});

var s= AppDomain.CurrentDomain.BaseDirectory;
builder.Services.AddControllers();
builder.Services.AddScoped<IContentService, ContentService>();
builder.Services.AddScoped<IVideoService, VideoService>();
builder.Services.AddScoped<IGenerator, JockerContentGenerator>();
builder.Services.AddScoped<IYouTubeFacade, YouTubeFacade>();
builder.Services.AddScoped<IImageService, ImageService>();
builder.Services.AddScoped<IChannelOptions, ChannelOptions>();

builder.Services.AddScoped<IYandexFacade, YandexFacade>();

builder.Services.AddScoped<IGenerator, JockerContentGenerator>();
builder.Services.AddScoped<IGenerator, ZodiacGenerator>();

builder.Services.AddHostedService<ContentJob>();

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