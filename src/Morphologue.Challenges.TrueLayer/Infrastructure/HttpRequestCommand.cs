using Morphologue.Challenges.TrueLayer.Interfaces.Infrastructure;
using System.Text;

namespace Morphologue.Challenges.TrueLayer.Infrastructure;

internal class HttpRequestCommand<TResponse> : IRequestCommand<TResponse>
    where TResponse : notnull
{
    private readonly string _url;
    private readonly Func<Stream, CancellationToken, Task<TResponse>> _mapper;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly string? _postBody;

    internal HttpRequestCommand(
        string url,
        Func<Stream, CancellationToken, Task<TResponse>> mapper,
        IHttpClientFactory httpClientFactory,
        string? postBody = null)
    {
        _url = url;
        _mapper = mapper;
        _httpClientFactory = httpClientFactory;
        _postBody = postBody;
    }

    public async Task<TResponse> ExecuteAsync(CancellationToken ct)
    {
        using var message = new HttpRequestMessage(HttpMethod.Get, _url);
        if (_postBody != null)
        {
            message.Method = HttpMethod.Post;
            message.Content = new StringContent(_postBody, Encoding.UTF8, "application/json");
        }
        using var response = await _httpClientFactory.CreateClient().SendAsync(message, ct);

        response.EnsureSuccessStatusCode();
        var stream = await response.Content.ReadAsStreamAsync(ct);
        return await _mapper.Invoke(stream, ct);
    }
}
