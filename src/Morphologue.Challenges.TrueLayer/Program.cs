using Microsoft.AspNetCore.Mvc;
using Morphologue.Challenges.TrueLayer;
using Morphologue.Challenges.TrueLayer.Interfaces.Application;
using Morphologue.Challenges.TrueLayer.Interfaces.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient();
builder.Services.Scan(scan =>
    scan.FromAssemblyOf<Program>()
        .AddClasses(classes => classes.WithAttribute<SingletonServiceAttribute>())
            .AsImplementedInterfaces()
            .WithSingletonLifetime());

var app = builder.Build();

app.UseSwagger();
if (app.Environment.IsDevelopment())
{
    app.UseSwaggerUI();
}

app.MapGet("/pokemon/{name}", ([FromRoute] string name, [FromServices] IPokemonCharacterisationService service, CancellationToken ct) =>
    service.CharacteriseAsync(name, false, ct));
app.MapGet("/pokemon/translated/{name}", ([FromRoute] string name, [FromServices] IPokemonCharacterisationService service, CancellationToken ct) =>
    service.CharacteriseAsync(name, true, ct));

app.Run();
