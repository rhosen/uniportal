namespace UniPortal.Middlewares
{
    public class NotFoundHandlingMiddleware
    {
        private readonly RequestDelegate _next;

        public NotFoundHandlingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            await _next(context);

            if (context.Response.StatusCode == 404 && !context.Response.HasStarted)
            {
                // Redirect to NotFound page
                context.Response.Redirect("/notfound");
            }
        }
    }
}
