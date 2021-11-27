using Morphologue.Challenges.TrueLayer.Interfaces.Infrastructure;
using System.Text.Json;

namespace Morphologue.Challenges.TrueLayer.Infrastructure;

[SingletonService]
public class FunTranslationsRequestCommandFactory : ITranslationRequestCommandFactory
{
    private readonly string _urlPrefix;
    private readonly IHttpClientFactory _httpClientFactory;

    public FunTranslationsRequestCommandFactory(IConfiguration config, IHttpClientFactory httpClientFactory)
    {
        _urlPrefix = config["FunTranslationsUrlPrefix"];
        _httpClientFactory = httpClientFactory;
    }

    public IRequestCommand<string> CreateTranslationCommand(string original, TranslationTarget target)
    {
        var urlSuffix = target switch
        {
            TranslationTarget.IncorrectEarlyModernEnglish => "shakespeare",
            TranslationTarget.Yoda => "yoda",
            _ => throw new NotSupportedException(target.ToString())
        };
        var body = JsonSerializer.Serialize(new Dictionary<string, string>
        {
            ["text"] = original
        });

        return new HttpRequestCommand<string>($"{_urlPrefix}/{urlSuffix}.json", MapTranslationAsync, _httpClientFactory, body);
    }

    private async Task<string> MapTranslationAsync(Stream rawResponse, CancellationToken ct)
    {
        var raw = await JsonSerializer.DeserializeAsync<Dictionary<string, JsonElement>>(rawResponse, cancellationToken: ct)
            ?? throw new JsonException("The translation response was null");
        if (!raw.TryGetValue("success", out var success) || success.GetProperty("total").GetInt32() != 1)
        {
            throw new HttpRequestException("The translation was unsuccessful");
        }

        return raw["contents"].GetProperty("translated").GetString()
            ?? throw new JsonException("The translation was null");
    }
}
