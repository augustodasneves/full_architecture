using Microsoft.AspNetCore.Http;
using System.Diagnostics;

namespace Shared.Telemetry;

public class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;
    private const string CorrelationIdHeader = "X-Correlation-Id";

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers[CorrelationIdHeader].FirstOrDefault();

        if (string.IsNullOrEmpty(correlationId))
        {
            correlationId = Activity.Current?.Id ?? Guid.NewGuid().ToString();
        }

        context.Response.Headers[CorrelationIdHeader] = correlationId;

        // Ensure the correlation id is associated with the current activity
        Activity.Current?.SetTag("correlation.id", correlationId);

        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            await _next(context);
        }
    }
}

// Simple LogContext mock if Serilog is not used, but usually it's better to use ILogger scope
public static class LogContext 
{
    public static IDisposable PushProperty(string key, object value) => new NoOpDisposable();
    private class NoOpDisposable : IDisposable { public void Dispose() {} }
}
