namespace Morphologue.Challenges.TrueLayer.Infrastructure
{
    public interface IRequestCommand<TResponse>
        where TResponse : notnull
    {
        Task<TResponse> ExecuteAsync();
    }
}
