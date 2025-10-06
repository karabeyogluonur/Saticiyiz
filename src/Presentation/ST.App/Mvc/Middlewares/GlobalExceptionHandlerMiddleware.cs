using System.Net;
using System.Text.Json;
using ST.Application.Exceptions; // Kendi özel exception sınıflarımız

namespace ST.App.Mvc.Middlewares;

public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;
    private readonly IHostEnvironment _env;

    public GlobalExceptionHandlerMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlerMiddleware> logger, IHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            // Bir sonraki middleware'i çağır. Eğer hata olmazsa, bu metot sorunsuz tamamlanır.
            await _next(context);
        }
        catch (Exception ex)
        {
            // Hata olursa, burada yakala.
            _logger.LogError(ex, "An unhandled exception has occurred. Message: {Message}", ex.Message);

            var response = context.Response;
            response.ContentType = "application/json";

            // Hatanın türüne göre HTTP durum kodunu belirle
            switch (ex)
            {
                case ValidationException e:
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    // FluentValidation hatalarını döndürebiliriz
                    await response.WriteAsync(JsonSerializer.Serialize(new { errors = e.Errors }));
                    return; // Hata cevabı yazıldığı için metodu sonlandır.

                case NotFoundException:
                    response.StatusCode = (int)HttpStatusCode.NotFound;
                    break;

                // Buraya başka özel hata türleri eklenebilir.
                // case UnauthorizedAccessException:
                //     response.StatusCode = (int)HttpStatusCode.Unauthorized;
                //     break;

                default:
                    response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    break;
            }

            // Dönecek olan hata mesajını ortamına göre belirle
            var errorResponse = new
            {
                message = ex.Message,
                // Production ortamındaysak, kullanıcıya teknik detayları (stack trace) sızdırmıyoruz.
                stackTrace = _env.IsDevelopment() ? ex.StackTrace : null
            };

            var result = JsonSerializer.Serialize(errorResponse);
            await response.WriteAsync(result);
        }
    }
}