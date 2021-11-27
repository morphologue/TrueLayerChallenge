using Morphologue.Challenges.TrueLayer.Interfaces.Application;
using Morphologue.Challenges.TrueLayer.Interfaces.Infrastructure;

namespace Morphologue.Challenges.TrueLayer.Application
{
    [SingletonService]
    internal class PokemonCharacterisationService : IPokemonCharacterisationService
    {
        private readonly IPokemonRequestCommandFactory _pokemonRequestCommandFactory;

        public PokemonCharacterisationService(IPokemonRequestCommandFactory pokemonRequestCommandFactory)
        {
            _pokemonRequestCommandFactory = pokemonRequestCommandFactory;
        }

        public async Task<PokemonCharacterisation> CharacteriseAsync(string name, bool translate, CancellationToken ct)
        {
            var pokemonResponse = await _pokemonRequestCommandFactory.CreatePokemonRequestCommand(name).ExecuteAsync(ct);
            var speciesResponse = await pokemonResponse.SpeciesRequestCommand.ExecuteAsync(ct);
            var description = speciesResponse.LocalisedDescriptions.FirstOrDefault(d => d.LanguageName == "en")?.Description
                ?? throw new NotSupportedException("The pokemon has no English description");
            return new(pokemonResponse.Name, speciesResponse.IsLegendary, description, speciesResponse.HabitatName);
        }
    }
}
