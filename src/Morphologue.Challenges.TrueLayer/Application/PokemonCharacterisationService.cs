using Morphologue.Challenges.TrueLayer.Interfaces.Application;
using Morphologue.Challenges.TrueLayer.Interfaces.Infrastructure;
using System.Text.RegularExpressions;

namespace Morphologue.Challenges.TrueLayer.Application;

[SingletonService]
internal class PokemonCharacterisationService : IPokemonCharacterisationService
{
    private readonly IPokemonRequestCommandFactory _pokemonRequestCommandFactory;
    private readonly ITranslationRequestCommandFactory _translationRequestCommandFactory;
    private readonly ILogger<PokemonCharacterisationService> _logger;

    public PokemonCharacterisationService(
        IPokemonRequestCommandFactory pokemonRequestCommandFactory,
        ITranslationRequestCommandFactory translationRequestCommandFactory,
        ILogger<PokemonCharacterisationService> logger)
    {
        _pokemonRequestCommandFactory = pokemonRequestCommandFactory;
        _translationRequestCommandFactory = translationRequestCommandFactory;
        _logger = logger;
    }

    public async Task<PokemonCharacterisation> CharacteriseAsync(string name, bool translate, CancellationToken ct)
    {
        var pokemonResponse = await _pokemonRequestCommandFactory.CreatePokemonRequestCommand(name).ExecuteAsync(ct);
        var speciesResponse = await pokemonResponse.SpeciesRequestCommand.ExecuteAsync(ct);
        var description = speciesResponse.LocalisedDescriptions.FirstOrDefault(d => d.LanguageName == "en")?.Description
            ?? throw new NotSupportedException("The pokemon has no English description");

        if (translate)
        {
            try
            {
                var deservesYoda = speciesResponse.HabitatName == "cave" || speciesResponse.IsLegendary;
                description = await TranslateAsync(description, deservesYoda, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception while translating for pokemon {PokemonName}: {CleanedDescription}",
                    name, description);
            }
        }

        var cleanedDescription = Regex.Replace(description, "\\s+", " ");
        return new(pokemonResponse.Name, cleanedDescription, speciesResponse.HabitatName, speciesResponse.IsLegendary);
    }

    private async Task<string> TranslateAsync(string original, bool deservesYoda, CancellationToken ct)
    {
        var target = deservesYoda ? TranslationTarget.Yoda : TranslationTarget.IncorrectEarlyModernEnglish;
        return await _translationRequestCommandFactory.CreateTranslationCommand(original, target).ExecuteAsync(ct);
    }
}
