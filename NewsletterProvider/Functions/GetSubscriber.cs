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

namespace NewsletterProvider.Functions;

public class GetSubscriber(ILogger<GetSubscriber> logger, IServiceProvider serviceProvider)
{
    private readonly ILogger<GetSubscriber> _logger = logger;
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    [Function("GetSubscriber")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req)
    {
        try
        {
            var body = await new StreamReader(req.Body).ReadToEndAsync();
            if (body != null)
            {
                try
                {
                    var requestBody = JsonConvert.DeserializeObject<SubscribeRequest>(body);

                    if (requestBody != null && requestBody.Email != null)
                    {
                        using var context = _serviceProvider.GetRequiredService<DataContext>();

                        var subscriber = await context.Subscribers.FirstOrDefaultAsync(x => x.Email == requestBody.Email);

                        if(subscriber != null)
                        {
                            return new OkResult();
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"JsonConvert.DeserializeObject<String>(body) :: {ex.Message}");
                }
            }

            return new BadRequestResult();
        }
        catch (Exception ex)
        {
            _logger.LogError($"StreamReader :: {ex.Message}");
        }

        return new BadRequestResult();
    }
}
