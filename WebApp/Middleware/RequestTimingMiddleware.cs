using System.Collections.Concurrent;
using System.Diagnostics;

namespace WebApp.Middleware;

public class RequestStats
{
    private long _total;
    private readonly ConcurrentDictionary<string, long> _paths = new(StringComparer.OrdinalIgnoreCase);

    public long Total => Interlocked.Read(ref _total);

    public long Hit(string path)
    {
        var normalized = string.IsNullOrEmpty(path) ? "/" : path;
        _paths.AddOrUpdate(normalized, 1, (_, count) => count + 1);
        return Interlocked.Increment(ref _total);
    }

    public IReadOnlyList<KeyValuePair<string, long>> TopPaths(int count = 5) =>
        _paths
            .OrderByDescending(kv => kv.Value)
            .ThenBy(kv => kv.Key)
            .Take(count)
            .ToList();
}

public class RequestTimingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestTimingMiddleware> _log;

    public RequestTimingMiddleware(RequestDelegate next, ILogger<RequestTimingMiddleware> log)
    {
        _next = next;
        _log = log;
    }

    public async Task InvokeAsync(HttpContext ctx, RequestStats stats)
    {
        if (ctx.Request.Path.StartsWithSegments("/blokada"))
        {
            ctx.Response.StatusCode = StatusCodes.Status403Forbidden;
            return;
        }

        var sw = Stopwatch.StartNew();
        var n = stats.Hit(ctx.Request.Path.Value ?? "/");

        ctx.Response.OnStarting(() =>
        {
            ctx.Response.Headers["X-Elapsed-Ms"] = sw.ElapsedMilliseconds.ToString();
            ctx.Response.Headers["X-Request-Count"] = n.ToString();
            return Task.CompletedTask;
        });

        await _next(ctx);

        _log.LogInformation("{Method} {Path} -> {Status} w {Ms} ms",
            ctx.Request.Method, ctx.Request.Path, ctx.Response.StatusCode, sw.ElapsedMilliseconds);
    }
}
