using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Models;
using System;
using System.Threading.Tasks;

namespace MailDelivery.Middleware
{
    public class ExceptionHandlerMiddleware
    {
        private readonly RequestDelegate next;
        private readonly ILogger<ExceptionHandlerMiddleware> logger;

        private static readonly int exceptionId = (int)LogType.ControllerException;

        public ExceptionHandlerMiddleware(RequestDelegate next, ILogger<ExceptionHandlerMiddleware> logger) =>
            (this.next, this.logger) = (next, logger);

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await next(context);
            }
            catch (BadHttpRequestException ex) when (ex.StatusCode == StatusCodes.Status413PayloadTooLarge)
            {
                context.Response.StatusCode = StatusCodes.Status413PayloadTooLarge;
                await context.Response.WriteAsJsonAsync(new { Reason = "Ошибка запроса.", Description = "Размер тела запроса привышает 1Мб." });

                string caller = context.User?.FindFirst("login")?.Value;
                logger.LogError(exceptionId, ex, "Ошибка сервиса. {Caller}", caller);
            }
            catch (Exception ex)
            {
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                await context.Response.WriteAsJsonAsync(new { Reason = "Ошибка сервиса.", Description = "При выполнении запроса вызникла ошибка. Обратитесь к разработчику." });
                logger.LogError(exceptionId, ex, "Ошибка сервиса.");
            }
        }
    }
}