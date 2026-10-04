using MessageHub.API.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace MessageHub.API.Tests;

/// <summary>
/// Тесты проверки Host/Origin (БЗ-07).
/// </summary>
public class CheckOriginMiddlewareTests
{
  private const string AllowedHost = "127.0.0.1:5000";
  private const string AllowedOrigin = "http://localhost:3000";

  [Fact]
  public async Task InvokeAsync_ForeignHost_Returns403AndSkipsNext()
  {
    var (status, nextCalled) = await Send("GET", "evil.com", null);

    Assert.Equal(StatusCodes.Status403Forbidden, status);
    Assert.False(nextCalled);
  }

  [Fact]
  public async Task InvokeAsync_AllowedHostWithoutPort_Returns403()
  {
    var (status, nextCalled) = await Send("GET", "127.0.0.1", null);

    Assert.Equal(StatusCodes.Status403Forbidden, status);
    Assert.False(nextCalled);
  }

  [Fact]
  public async Task InvokeAsync_GetWithAllowedOrigin_PassesToNext()
  {
    var (status, nextCalled) = await Send("GET", AllowedHost, AllowedOrigin);

    Assert.Equal(StatusCodes.Status200OK, status);
    Assert.True(nextCalled);
  }

  [Fact]
  public async Task InvokeAsync_GetWithoutOrigin_PassesToNext()
  {
    var (status, nextCalled) = await Send("GET", AllowedHost, null);

    Assert.Equal(StatusCodes.Status200OK, status);
    Assert.True(nextCalled);
  }

  [Fact]
  public async Task InvokeAsync_PostWithoutOrigin_Returns403AndSkipsNext()
  {
    var (status, nextCalled) = await Send("POST", AllowedHost, null);

    Assert.Equal(StatusCodes.Status403Forbidden, status);
    Assert.False(nextCalled);
  }

  [Fact]
  public async Task InvokeAsync_PostWithForeignOrigin_Returns403AndSkipsNext()
  {
    var (status, nextCalled) = await Send("POST", AllowedHost, "https://evil.com");

    Assert.Equal(StatusCodes.Status403Forbidden, status);
    Assert.False(nextCalled);
  }

  [Fact]
  public async Task InvokeAsync_PostWithAllowedOrigin_PassesToNext()
  {
    var (status, nextCalled) = await Send("POST", AllowedHost, AllowedOrigin);

    Assert.Equal(StatusCodes.Status200OK, status);
    Assert.True(nextCalled);
  }

  [Fact]
  public async Task InvokeAsync_GetWithForeignOrigin_Returns403()
  {
    var (status, nextCalled) = await Send("GET", AllowedHost, "https://evil.com");

    Assert.Equal(StatusCodes.Status403Forbidden, status);
    Assert.False(nextCalled);
  }

  [Theory]
  [InlineData("null")]
  [InlineData("http://localhost:3000.evil.com")]
  [InlineData("http://localhost:3000/")]
  [InlineData("https://localhost:3000")]
  public async Task InvokeAsync_OriginLookalike_Returns403(string origin)
  {
    var (status, nextCalled) = await Send("GET", AllowedHost, origin);

    Assert.Equal(StatusCodes.Status403Forbidden, status);
    Assert.False(nextCalled);
  }

  [Fact]
  public async Task InvokeAsync_OriginInDifferentCase_PassesToNext()
  {
    var (status, nextCalled) = await Send("POST", AllowedHost, "HTTP://LocalHost:3000");

    Assert.Equal(StatusCodes.Status200OK, status);
    Assert.True(nextCalled);
  }

  [Fact]
  public async Task InvokeAsync_NoSecuritySection_RejectsEverything()
  {
    var (status, nextCalled) = await Send("GET", AllowedHost, null, new ConfigurationBuilder().Build());

    Assert.Equal(StatusCodes.Status403Forbidden, status);
    Assert.False(nextCalled);
  }

  private static async Task<(int Status, bool NextCalled)> Send(string method, string host, string? origin, IConfiguration? configuration = null)
  {
    var nextCalled = false;
    var middleware = new CheckOriginMiddleware(_ =>
    {
      nextCalled = true;
      return Task.CompletedTask;
    }, configuration ?? CreateConfiguration());

    var context = new DefaultHttpContext();
    context.Request.Method = method;
    context.Request.Host = new HostString(host);
    if (origin is not null)
    {
      context.Request.Headers.Origin = origin;
    }

    await middleware.InvokeAsync(context);

    return (context.Response.StatusCode, nextCalled);
  }

  private static IConfiguration CreateConfiguration() =>
    new ConfigurationBuilder()
      .AddInMemoryCollection(new Dictionary<string, string?>
      {
        ["Security:AllowedHosts:0"] = AllowedHost,
        ["Security:AllowedHosts:1"] = "localhost:5000",
        ["Security:AllowedOrigins:0"] = AllowedOrigin,
        ["Security:AllowedOrigins:1"] = "http://127.0.0.1:3000"
      })
      .Build();
}
