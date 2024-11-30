using System.Net.Http.Headers;
using Alex.YouTube.Joker.Host;
using Alex.YouTube.Joker.Host.Facades;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();
builder.Services.AddScoped<IGptFacade, GptFacade>();

builder.Services.AddHttpClient<GptFacade>(client =>
{ 
    var openAiApiKey = builder.Configuration["OpenAI:ApiKey"];
    client.BaseAddress = new Uri("https://api.openai.com/");
    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", openAiApiKey);
});

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