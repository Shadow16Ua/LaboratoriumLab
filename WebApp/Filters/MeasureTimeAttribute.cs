using System.Diagnostics;
using Microsoft.AspNetCore.Mvc.Filters;

namespace WebApp.Filters;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class MeasureTimeAttribute : ActionFilterAttribute
{
    private const string StopwatchKey = "__MeasureTime_Stopwatch";

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var logger = context.HttpContext.RequestServices.GetService<ILogger<MeasureTimeAttribute>>();
        logger?.LogInformation("[MeasureTime Order={Order}] Executing {Action}", Order, context.ActionDescriptor.DisplayName);

        var sw = Stopwatch.StartNew();
        context.HttpContext.Items[StopwatchKey] = sw;

        context.HttpContext.Response.OnStarting(() =>
        {
            if (context.HttpContext.Items[StopwatchKey] is Stopwatch running)
            {
                context.HttpContext.Response.Headers["X-Action-Ms"] = running.ElapsedMilliseconds.ToString();
            }
            return Task.CompletedTask;
        });
    }

    public override void OnActionExecuted(ActionExecutedContext context)
    {
        if (context.HttpContext.Items[StopwatchKey] is Stopwatch sw)
        {
            sw.Stop();
            if (!context.HttpContext.Response.HasStarted)
            {
                context.HttpContext.Response.Headers["X-Action-Ms"] = sw.ElapsedMilliseconds.ToString();
            }

            var logger = context.HttpContext.RequestServices.GetService<ILogger<MeasureTimeAttribute>>();
            logger?.LogInformation("[MeasureTime Order={Order}] Executed {Action} w {Ms} ms",
                Order, context.ActionDescriptor.DisplayName, sw.ElapsedMilliseconds);
        }
    }
}
