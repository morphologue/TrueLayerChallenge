using Morphologue.Challenges.TrueLayer.Application;

namespace Morphologue.Challenges.TrueLayer
{
    public class NotFoundMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<NotFoundMiddleware> _logger;

        public NotFoundMiddleware(RequestDelegate next, ILogger<NotFoundMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (NotFoundException ex)
            {
                _logger.LogInformation(ex, "Handling {ExceptionTypeName} during request {RequestMethod} request to {RequestPath}",
                    ex.GetType().Name, context.Request.Method, context.Request.Path);
                context.Response.Clear();
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                context.Response.ContentType = "text/plain";
                await context.Response.WriteAsync("Not Found");
            }
        }
    }
}
