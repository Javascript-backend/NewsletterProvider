using Data.Contexts;
using Data.Entities;
using Data.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace NewsletterProvider.Functions
{
    public class UpdateSubscriber(ILogger<UpdateSubscriber> logger, IServiceProvider serviceProvider)
    {
        private readonly ILogger<UpdateSubscriber> _logger = logger;
        private readonly IServiceProvider _serviceProvider = serviceProvider;

        [Function("UpdateSubscriber")]
        public async Task <IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "put")] HttpRequest req)
        {
            try
            {
                var body = await new StreamReader(req.Body).ReadToEndAsync();

                if(body != null)
                {
                    try
                    {
                        var requestBody = JsonConvert.DeserializeObject<SubscribeEntity>(body);

                        if(requestBody != null)
                        {
                            using var context = _serviceProvider.GetRequiredService<DataContext>();

                            var subscriber = await context.Subscribers.FirstOrDefaultAsync(x => x.Email == requestBody.Email);

                            if(subscriber != null)
                            {
                                context.Entry(subscriber).CurrentValues.SetValues(requestBody);

                                await context.SaveChangesAsync();

                                return new OkResult();
                            }

                            return new BadRequestResult();
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"JsonConvert.DeserializeObject<SubscribeEntity>(body) :: {ex.Message}");
                    }
                }

                return new BadRequestResult();
            }
            catch (Exception ex)
            {
                logger.LogError($"StreamReader :: {ex.Message}");
            }

            return new BadRequestResult();
        }
    }
}
