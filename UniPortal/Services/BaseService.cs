using UniPortal.Constants;
using UniPortal.Data;
using UniPortal.Data.Entities;
using UniPortal.Services.Infrastructures;

namespace UniPortal.Services
{
    public abstract class BaseService<TEntity> where TEntity : IEntity
    {
        protected readonly UniPortalContext _context;
        protected readonly LogService _logService;

        protected BaseService(UniPortalContext context, LogService logService)
        {
            _context = context;
            _logService = logService;
        }

        protected async Task LogAsync(Guid? userId, ActionType actionType, string entity, Guid? entityId, object? details = null)
        {
            await _logService.CreateAsync(userId, actionType, $"{actionType} {entity}", entity, entityId, details);
        }
    }

}
