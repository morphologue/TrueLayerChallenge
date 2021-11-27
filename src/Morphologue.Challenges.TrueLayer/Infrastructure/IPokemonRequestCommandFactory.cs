namespace Morphologue.Challenges.TrueLayer.Infrastructure
{
    public interface IPokemonRequestCommandFactory
    {
        IRequestCommand<PokemonResponse> CreatePokemonRequestCommand(string name);
    }

    public record PokemonResponse(string Name, IRequestCommand<PokemonSpeciesResponse> SpeciesRequestCommand);
    
    public record PokemonSpeciesResponse(bool IsLegendary, string Description, string? HabitatName);
}
