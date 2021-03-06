namespace Morphologue.Challenges.TrueLayer.Interfaces.Infrastructure;

public interface IPokemonRequestCommandFactory
{
    IRequestCommand<PokemonResponse> CreatePokemonRequestCommand(string name);
}

public record PokemonResponse(string Name, IRequestCommand<PokemonSpeciesResponse> SpeciesRequestCommand);

public record PokemonSpeciesResponse(bool IsLegendary, IEnumerable<LocalisedDescription> LocalisedDescriptions, string? HabitatName);

public record LocalisedDescription(string LanguageName, string Description);
