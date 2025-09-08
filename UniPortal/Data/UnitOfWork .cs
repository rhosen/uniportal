using Microsoft.EntityFrameworkCore.Storage;

namespace UniPortal.Data
{
    public class UnitOfWork : IUnitOfWork, IAsyncDisposable
    {
        private readonly UniPortalContext _context;
        private IDbContextTransaction? _transaction;

        public UnitOfWork(UniPortalContext context)
        {
            _context = context;
        }

        public UniPortalContext Context => _context;

        /// <summary>
        /// Commits all changes in the context within a transaction.
        /// Works for single or multiple entities.
        /// </summary>
        public async Task CommitAsync()
        {
            if (!_context.ChangeTracker.HasChanges())
                return; // nothing to save

            // Start a transaction if not already started
            _transaction ??= await _context.Database.BeginTransactionAsync();

            try
            {
                await _context.SaveChangesAsync();
                await _transaction.CommitAsync();
            }
            catch
            {
                await _transaction.RollbackAsync();
                throw;
            }
            finally
            {
                await DisposeTransactionAsync();
            }
        }

        /// <summary>
        /// Rollback transaction manually if needed
        /// </summary>
        public async Task RollbackAsync()
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync();
                await DisposeTransactionAsync();
            }
        }

        private async Task DisposeTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (_transaction != null)
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }

            await _context.DisposeAsync();
        }
    }
}
