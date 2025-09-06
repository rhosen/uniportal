using UniPortal.Services;

namespace UniPortal.Middleware
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

                // Handle the response
                await HandleExceptionAsync(context, ticketId);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, string ticketId)
        {
            context.Response.StatusCode = 500;
            context.Response.ContentType = "text/html";

            // Friendly HTML response with ticket ID
            return context.Response.WriteAsync($@"
                <div style='text-align:center; margin-top:50px; font-family:sans-serif;'>
                    <h1>Oops! Something went wrong.</h1>
                    <p>Our team has been notified. Please share this ticket ID with support:</p>
                    <h3 style='color:#dc3545'>{ticketId}</h3>
                    <p><a href='/'>Go back to Dashboard</a></p>
                </div>");
        }
    }
}
