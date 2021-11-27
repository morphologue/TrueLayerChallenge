namespace Morphologue.Challenges.TrueLayer.Infrastructure
{
    internal class HttpRequestCommand<TResponse> : IRequestCommand<TResponse>
        where TResponse : notnull
    {
        private readonly string _url;
        private readonly Func<Stream, Task<TResponse>> _mapper;
        private readonly IHttpClientFactory _httpClientFactory;

        internal HttpRequestCommand(string url, Func<Stream, Task<TResponse>> mapper, IHttpClientFactory httpClientFactory)
        {
            _url = url;
            _mapper = mapper;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<TResponse> ExecuteAsync()
        {
            var response = await _httpClientFactory.CreateClient().GetAsync(_url);
            response.EnsureSuccessStatusCode();
            var stream = await response.Content.ReadAsStreamAsync();
            return await _mapper.Invoke(stream);
        }
    }
}
