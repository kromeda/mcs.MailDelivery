using MailDelivery.Models.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Models;
using System.Net;
using System.Threading.Tasks;

namespace MailDelivery.Filters
{
    public class DistributionExistAttribute : TypeFilterAttribute
    {
        public DistributionExistAttribute() : base(typeof(DistributionExistFilter)) { }

        public class DistributionExistFilter : IAsyncActionFilter
        {
            private readonly ILogger<DistributionExistFilter> logger;
            private readonly IDbRepository repository;

            public DistributionExistFilter(ILogger<DistributionExistFilter> logger, IDbRepository repository)
            {
                this.repository = repository;
                this.logger = logger;
            }

            public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
            {
                if (context.ActionArguments.TryGetValue("distributionId", out object parameter))
                {
                    int distributionId = (int)parameter;
                    var distribution = await repository.FindDistributionAsync(distributionId);

                    if (distribution != null)
                    {
                        await next.Invoke();
                    }
                    else
                    {
                        WriteBadResponse(context, HttpStatusCode.NotFound, "Рассылка не найдена", "Рассылка с переданным идентификатором не зарегистрирована.");
                        logger.LogWarning((int)LogType.MailCompanyNotFound, "Рассылка с идентификатором {Id} не найдена.", distributionId);
                    }
                }
                else
                {
                    WriteBadResponse(context, HttpStatusCode.BadRequest, "Идентификатор не найден", "Идентификатор рассылки не найден в запросе.");
                    logger.LogWarning((int)LogType.MissingParameter, "Не удалось получить значение параметра \'distributionId\'.");
                }
            }

            private void WriteBadResponse(ActionExecutingContext context, HttpStatusCode code, string reason, string description)
            {
                context.HttpContext.Response.ContentType = "application/json";
                context.HttpContext.Response.StatusCode = (int)code;
                context.Result = new ObjectResult(new { Reason = reason, Description = description });
            }
        }
    }
}