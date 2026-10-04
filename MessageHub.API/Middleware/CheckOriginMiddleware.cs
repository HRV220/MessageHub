namespace MessageHub.API.Middleware;

/// <summary>
/// Проверяет заголовки <c>Host</c> и <c>Origin</c> входящего запроса и отклоняет чужие (БЗ-07).
/// Защищает от DNS rebinding и обращения к API со сторонних сайтов, открытых в том же браузере.
/// CORS не подключается: без заголовков <c>Access-Control-Allow-*</c> браузер не отдаст ответ чужой странице.
/// </summary>
public class CheckOriginMiddleware
{
  private readonly HashSet<string> _allowedHosts = new(StringComparer.OrdinalIgnoreCase);
  private readonly HashSet<string> _allowedOrigins = new(StringComparer.OrdinalIgnoreCase);
  private readonly RequestDelegate _next;

  public CheckOriginMiddleware(RequestDelegate next, IConfiguration configuration)
  {
    _next = next;
    foreach (var host in configuration.GetSection("Security:AllowedHosts").Get<string[]>() ?? [])
    {
      _allowedHosts.Add(host);
    }

    foreach (var origin in configuration.GetSection("Security:AllowedOrigins").Get<string[]>() ?? [])
    {
      _allowedOrigins.Add(origin);
    }
  }

  public async Task InvokeAsync(HttpContext context)
  {
    if (!_allowedHosts.Contains(context.Request.Host.Value.ToString()))
    {
      context.Response.StatusCode = StatusCodes.Status403Forbidden;
      return;
    }
    var origin = context.Request.Headers.Origin.ToString();
    var methods = context.Request.Method;
    if (string.IsNullOrWhiteSpace(origin))
    {
      if (!(HttpMethods.IsGet(methods) || HttpMethods.IsOptions(methods) || HttpMethods.IsHead(methods)))
      {
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        return;
      }
    }
    else if (!_allowedOrigins.Contains(origin))
    {
      context.Response.StatusCode = StatusCodes.Status403Forbidden;
      return;
    }
    await _next(context);
  }

}
