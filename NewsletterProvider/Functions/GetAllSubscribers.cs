using Data.Contexts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace NewsletterProvider.Functions
{
    public class GetAllSubscribers(ILogger<GetAllSubscribers> logger, IServiceProvider serviceProvider)
    {
        private readonly ILogger<GetAllSubscribers> _logger = logger;
        private readonly IServiceProvider _serviceProvider = serviceProvider;

        [Function("GetAllSubscribers")]
        public IActionResult Run([HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequest req)
        {
            try
            {

                using var context = _serviceProvider.GetRequiredService<DataContext>();

                var subscriberList = context.Subscribers.ToList();

                if(subscriberList != null)
                {
                    return new OkObjectResult(subscriberList);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"StreamReader :: {ex.Message}");
            }
            return new BadRequestResult();
        }
    }
}
