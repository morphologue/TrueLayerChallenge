namespace Morphologue.Challenges.TrueLayer.Interfaces.Infrastructure
{
    public interface IRequestCommand<TResponse>
        where TResponse : notnull
    {
        Task<TResponse> ExecuteAsync(CancellationToken ct);
    }
}
