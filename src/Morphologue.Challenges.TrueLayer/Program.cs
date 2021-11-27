using Morphologue.Challenges.TrueLayer;
using Morphologue.Challenges.TrueLayer.Infrastructure;

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
    app.UseDeveloperExceptionPage();
}

// TODO: Remove this. It's just a POC.
var mewtwo = await app.Services.GetRequiredService<IPokemonRequestCommandFactory>().CreatePokemonRequestCommand("mewtwo").ExecuteAsync();
var species = await mewtwo.SpeciesRequestCommand.ExecuteAsync();
Console.WriteLine(species);

app.Run();
