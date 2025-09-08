using Microsoft.AspNetCore.Http;
using UniPortal.Services.Infrastructures;

namespace UniPortal.Middlewares
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly FileLogService _fileLogService;

        public ExceptionHandlingMiddleware(RequestDelegate next, FileLogService fileLogService)
        {
            _next = next;
            _fileLogService = fileLogService;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context); // Call the next middleware
            }
            catch (Exception ex)
            {
                // Log exception and generate ticket ID
                var ticketId = _fileLogService.LogError(ex, $"Path: {context.Request.Path}");

                // Redirect to the Error Razor page
                var errorUrl = $"/error?ticketId={ticketId}";
                context.Response.Redirect(errorUrl);
            }
        }
    }
}
