using UniPortal.Data;

public interface IUnitOfWork : IAsyncDisposable
{
    UniPortalContext Context { get; }
    Task CommitAsync();
    Task RollbackAsync();
}
