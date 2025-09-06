using Serilog;

namespace UniPortal.Services
{
    public class FileLogService
    {
        public string LogError(Exception ex, string contextInfo = null)
        {
            var ticketId = Guid.NewGuid().ToString();

            if (!string.IsNullOrEmpty(contextInfo))
                Log.Error(ex, "Ticket {TicketId}: Exception occurred. Context: {ContextInfo}", ticketId, contextInfo);
            else
                Log.Error(ex, "Ticket {TicketId}: Exception occurred.", ticketId);

            return ticketId;
        }

        public void LogInfo(string message, string contextInfo = null)
        {
            if (!string.IsNullOrEmpty(contextInfo))
                Log.Information("Info: {Message}. Context: {ContextInfo}", message, contextInfo);
            else
                Log.Information("Info: {Message}", message);
        }

        public void LogWarning(string message, string contextInfo = null)
        {
            if (!string.IsNullOrEmpty(contextInfo))
                Log.Warning("Warning: {Message}. Context: {ContextInfo}", message, contextInfo);
            else
                Log.Warning("Warning: {Message}", message);
        }
    }
}
