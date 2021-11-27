using System.Text.Json;
using System.Web;

namespace Morphologue.Challenges.TrueLayer.Infrastructure
{
    [SingletonService]
    internal class PokeAPIRequestCommandFactory : IPokemonRequestCommandFactory
    {
        private readonly string _urlPrefix;
        private readonly IHttpClientFactory _httpClientFactory;

        public PokeAPIRequestCommandFactory(IConfiguration config, IHttpClientFactory httpClientFactory)
        {
            _urlPrefix = config["PokeAPIPokemonUrlPrefix"];
            _httpClientFactory = httpClientFactory;
        }

        public IRequestCommand<PokemonResponse> CreatePokemonRequestCommand(string name)
        {
            var urlEncodedName = HttpUtility.UrlEncode(name);
            return new HttpRequestCommand<PokemonResponse>($"{_urlPrefix}/{urlEncodedName}/", MapPokemonAsync, _httpClientFactory);
        }

        private async Task<PokemonResponse> MapPokemonAsync(Stream rawResponse)
        {
            var raw = await JsonSerializer.DeserializeAsync<JsonElement>(rawResponse);
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

        private async Task<PokemonSpeciesResponse> MapSpeciesAsync(Stream rawResponse)
        {
            var raw = await JsonSerializer.DeserializeAsync<JsonElement>(rawResponse);
            var descriptions = GetDescriptions(raw.GetProperty("flavor_text_entries"));
            var habitatName = raw.GetProperty("habitat").GetProperty("name").GetString();
            var isLegendary = raw.GetProperty("is_legendary").GetBoolean();
            return new(isLegendary, descriptions, habitatName);
        }

        private IEnumerable<LocalisedDescription> GetDescriptions(JsonElement flavourTextEntries)
        {
            return flavourTextEntries
                .EnumerateArray()
                .Select(e => new LocalisedDescription(
                    LanguageName: e.GetProperty("flavor_text").GetString()
                        ?? throw new JsonException("The language name of a description of the pokemon was null"),
                    Description: e.GetProperty("language").GetProperty("name").GetString()
                        ?? throw new JsonException("A description of the pokemon was null")));
        }
    }
}
