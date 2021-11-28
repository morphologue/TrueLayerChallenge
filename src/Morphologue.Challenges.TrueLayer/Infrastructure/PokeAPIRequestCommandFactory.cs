using Morphologue.Challenges.TrueLayer.Interfaces.Infrastructure;
using System.Text.Json;
using System.Web;

namespace Morphologue.Challenges.TrueLayer.Infrastructure;

[SingletonService]
internal class PokeAPIRequestCommandFactory : IPokemonRequestCommandFactory
{
    private readonly IConfiguration _config;
    private readonly IHttpClientFactory _httpClientFactory;

    public PokeAPIRequestCommandFactory(IConfiguration config, IHttpClientFactory httpClientFactory)
    {
        _config = config;
        _httpClientFactory = httpClientFactory;
    }

    private string UrlPrefix => _config["PokeAPIPokemonUrlPrefix"];

    public IRequestCommand<PokemonResponse> CreatePokemonRequestCommand(string name)
    {
        var urlEncodedName = HttpUtility.UrlEncode(name);
        return new HttpRequestCommand<PokemonResponse>($"{UrlPrefix}/{urlEncodedName}/", MapPokemonAsync, _httpClientFactory);
    }

    private async Task<PokemonResponse> MapPokemonAsync(Stream rawResponse, CancellationToken ct)
    {
        var raw = await JsonSerializer.DeserializeAsync<JsonElement>(rawResponse, cancellationToken: ct);
        var name = raw.GetProperty("name").GetString()
            ?? throw new JsonException($"The pokemon name was null");
        var speciesUrl = raw.GetProperty("species").GetProperty("url").GetString()
            ?? throw new JsonException($"The pokemon's species URL was null");
        return new(name, CreateSpeciesRequestCommand(speciesUrl));
    }

    private IRequestCommand<PokemonSpeciesResponse> CreateSpeciesRequestCommand(string speciesUrl)
    {
        return new HttpRequestCommand<PokemonSpeciesResponse>(speciesUrl, MapSpeciesAsync, _httpClientFactory);
    }

    private async Task<PokemonSpeciesResponse> MapSpeciesAsync(Stream rawResponse, CancellationToken ct)
    {
        var raw = await JsonSerializer.DeserializeAsync<JsonElement>(rawResponse, cancellationToken: ct);
        var descriptions = GetDescriptions(raw.GetProperty("flavor_text_entries"));
        var habitatName = raw.GetProperty("habitat").GetProperty("name").GetString();
        var isLegendary = raw.GetProperty("is_legendary").GetBoolean();
        return new(isLegendary, descriptions, habitatName);
    }

    private static IEnumerable<LocalisedDescription> GetDescriptions(JsonElement flavourTextEntries)
    {
        return flavourTextEntries
            .EnumerateArray()
            .Select(e => new LocalisedDescription(
                LanguageName: e.GetProperty("language").GetProperty("name").GetString()
                    ?? throw new JsonException("The language name of a description of the pokemon was null"),
                Description: e.GetProperty("flavor_text").GetString()
                    ?? throw new JsonException("A description of the pokemon was null")));
    }
}
