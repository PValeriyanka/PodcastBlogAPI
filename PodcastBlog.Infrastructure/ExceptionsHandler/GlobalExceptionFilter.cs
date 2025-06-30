using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PodcastBlog.Infrastructure.ExceptionsHandler.Exceptions;
using System.ComponentModel.DataAnnotations;

namespace PodcastBlog.Infrastructure.ExceptionsHandler
{
    public class GlobalExceptionFilter : IExceptionFilter
    {
        private readonly ILogger<GlobalExceptionFilter> _logger;

        public GlobalExceptionFilter(ILogger<GlobalExceptionFilter> logger)
        {
            _logger = logger;
        }

        public void OnException(ExceptionContext context)
        {
            var ex = context.Exception;

            _logger.LogError(ex, "Обработано глобальное исключение");

            var response = ex switch
            {
                NotFoundException => new ObjectResult(new { message = ex.Message }) { StatusCode = 404 },
                ForbiddenException => new ObjectResult(new { message = ex.Message }) { StatusCode = 403 },
                AuthException => new ObjectResult(new { message = ex.Message }) { StatusCode = 409 },
                MediaException => new ObjectResult(new {message = ex.Message}) { StatusCode = 422},

                DbUpdateException => new ObjectResult(new { message = "Ошибка базы данных" }) { StatusCode = 409 },
                ValidationException => new ObjectResult(new { message = ex.Message }) { StatusCode = 422 },
                UnauthorizedAccessException => new ObjectResult(new { message = "Недостаточно прав доступа" }) { StatusCode = 403 },
                ArgumentNullException => new ObjectResult(new { message = "Отсутствуют обязательные параметры" }) { StatusCode = 400 },

                _ => new ObjectResult(new { message = "Внутренняя ошибка сервера" }) { StatusCode = 500 }
            };

            context.Result = response;
            context.ExceptionHandled = true;
        }
    }
}
