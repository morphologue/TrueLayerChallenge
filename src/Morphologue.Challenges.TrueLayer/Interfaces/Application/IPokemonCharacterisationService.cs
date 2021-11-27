namespace Morphologue.Challenges.TrueLayer.Interfaces.Application
{
    public interface IPokemonCharacterisationService
    {
        Task<PokemonCharacterisation> CharacteriseAsync(string name, bool translate, CancellationToken ct);
    }

    public record PokemonCharacterisation(string Name, bool IsLegendary, string Description, string? HabitatName);
}
