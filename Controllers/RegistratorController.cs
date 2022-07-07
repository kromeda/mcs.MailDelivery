using MailDelivery.Filters;
using MailDelivery.Models;
using MailDelivery.Models.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Models;
using Models.MailDelivery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MailDelivery.Controllers
{
    [ApiController]
    [Route("api/register")]
    public class RegistratorController : ControllerBase
    {
        private readonly ILogger<RegistratorController> logger;
        private readonly IDbRepository repository;

        public RegistratorController(ILogger<RegistratorController> logger, IDbRepository repository)
        {
            this.logger = logger;
            this.repository = repository;
        }

        [HttpPost("distribution")]
        public async Task<ActionResult<int>> CreateDistribution(DistributionArgs args)
        {
            Template template = await repository.FindTemplateAsync(args.Template);
            Distribution distribution = new Distribution
            {
                FromAddress = args.SenderAddress,
                FromAlias = args.SenderAlias,
                Status = StatusType.Registered,
                TemplateId = template.Id,
            };

            var id = await repository.CreateDistributionAsync(distribution);
            logger.LogInformation((int)LogType.MailDeliveyCompanyCreated, "Зарегистрированна новая рассылка.");

            return Ok(id);
        }

        [DistributionExist]
        [HttpPut("finished/{distributionId:int}")]
        public async Task<ActionResult> TransmissionFinished(int distributionId)
        {
            await repository.MarkDistributionAsync(distributionId, StatusType.Transmited);
            logger.LogInformation("Передача писем для рассылки с Id = {DistributionId} завершена.", distributionId);

            return Ok();
        }

        [RequestSizeLimit(1_000_000)]
        [DistributionExist]
        [HttpPost("letters/{distributionId:int}")]
        public async Task<ActionResult> Post(int distributionId, List<LetterArgs> args)
        {
            var letters = args.Select(arg => new Letter
            {
                DistributionId = distributionId,
                Status = StatusType.Registered,
                ToAddress = arg.ReceiverAddress,
                Subject = arg.Subject,
                Arguments = arg.Arguments
            });

            await repository.RegisterLettersAsync(letters);
            logger.LogInformation((int)LogType.LettersRegistered,
                "Зарегистрировано {LettersCount} писем для доставки, по рассылке №{DistributionId}", letters.Count(), distributionId);

            return Ok();
        }
    }
}