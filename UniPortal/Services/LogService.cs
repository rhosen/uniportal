using System.Text.Json;
using UniPortal.Constants;
using UniPortal.Data;
using UniPortal.Data.Entities.UniPortal.Data.Entities;

namespace UniPortal.Services
{
    public class LogService
    {
        private readonly UniPortalContext _context;
        private readonly FileLogService _fileLogService;

        public LogService(UniPortalContext context, FileLogService fileLogService)
        {
            _context = context;
            _fileLogService = fileLogService;
        }

        /// <summary>
        /// Logs normal actions to DB and optionally to file
        /// </summary>
        public async Task CreateAsync(
            Guid? accountId,
            ActionType actionType,
            string actionDescription,
            string? entity = null,
            Guid? entityId = null,
            object? details = null)
        {
            // DB logging
            var log = new Log
            {
                AccountId = accountId,
                ActionType = actionType,
                Action = actionDescription,
                Entity = entity,
                EntityId = entityId,
                Details = details != null ? JsonSerializer.Serialize(details) : null,
                Timestamp = DateTime.UtcNow
            };

            _context.Logs.Add(log);
            await _context.SaveChangesAsync();

            // File logging via FileLogService
            var logDetails = details != null ? JsonSerializer.Serialize(details, new JsonSerializerOptions { WriteIndented = true }) : null;
            var contextInfo = entityId.HasValue ? $"Entity: {entity}, EntityId: {entityId}" : $"Entity: {entity ?? "null"}";
            _fileLogService.LogInfo($"{actionType}: {actionDescription}", $"{contextInfo} | AccountId: {accountId} | Details: {logDetails}");
        }
    }
}
