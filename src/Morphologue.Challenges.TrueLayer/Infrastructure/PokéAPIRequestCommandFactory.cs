using System.Text.Json;
using System.Web;

namespace Morphologue.Challenges.TrueLayer.Infrastructure
{
    [SingletonService]
    internal class PokéAPIRequestCommandFactory : IPokemonRequestCommandFactory
    {
        private readonly string _urlPrefix;
        private readonly IHttpClientFactory _httpClientFactory;

        public PokéAPIRequestCommandFactory(IConfiguration config, IHttpClientFactory httpClientFactory)
        {
            _urlPrefix = config.GetValue<string>("PokeAPIPokemonUrlPrefix");
            _httpClientFactory = httpClientFactory;
        }

        public IRequestCommand<PokemonResponse> CreatePokemonRequestCommand(string name)
        {
            var urlEncodedName = HttpUtility.UrlEncode(name);
            return new HttpRequestCommand<PokemonResponse>(
                url: $"{_urlPrefix}/{urlEncodedName}/",
                mapper: r => MapPokemon(name, r),
                _httpClientFactory);
        }

        private PokemonResponse MapPokemon(string pokemonName, string rawResponse)
        {
            var raw = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(rawResponse)
                ?? throw new HttpRequestException($"The response for pokemon {pokemonName} was null");
            var name = raw["name"].GetString()
                ?? throw new JsonException($"The name of pokemon {pokemonName} was null");
            var speciesUrl = raw["species"].GetProperty("url").GetString()
                ?? throw new JsonException($"The species URL of pokemon {pokemonName} was null");
            return new(name, CreateSpeciesRequestCommand(pokemonName, speciesUrl));
        }

        private IRequestCommand<PokemonSpeciesResponse> CreateSpeciesRequestCommand(string pokemonName, string speciesUrl)
        {
            return new HttpRequestCommand<PokemonSpeciesResponse>(
                url: speciesUrl,
                mapper: r => MapSpecies(pokemonName, r),
                _httpClientFactory);
        }

        private PokemonSpeciesResponse MapSpecies(string pokemonName, string rawResponse)
        {
            var raw = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(rawResponse)
                ?? throw new HttpRequestException($"The species response for pokemon {pokemonName} was null");
            var description = GetEnglishDescription(pokemonName, raw["flavor_text_entries"].EnumerateArray());
            var habitatName = raw["habitat"].GetProperty("name").GetString();
            var isLegendary = raw["is_legendary"].GetBoolean();
            return new(isLegendary, description, habitatName);
        }

        private string GetEnglishDescription(string pokemonName, JsonElement.ArrayEnumerator flavourTextEntries)
        {
            foreach (var entry in flavourTextEntries)
            {
                if (entry.GetProperty("language").GetProperty("name").GetString() == "en")
                {
                    return entry.GetProperty("flavor_text").GetString()
                        ?? throw new JsonException($"The description of pokemon {pokemonName} was null");
                }
            }
            throw new JsonException($"The pokemon {pokemonName} had no English description");
        }
    }
}
