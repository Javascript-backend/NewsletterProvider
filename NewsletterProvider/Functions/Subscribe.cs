using Data.Contexts;
using Data.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace NewsletterProvider.Functions
{
    public class Subscribe(ILogger<Subscribe> logger, IServiceProvider serviceProvider)
    {
        private readonly ILogger<Subscribe> _logger = logger;
        private readonly IServiceProvider _serviceProvider = serviceProvider;

        [Function("Subscribe")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req)
        {
            try
            {
                using var _context = _serviceProvider.GetRequiredService<DataContext>();

                var body = await new StreamReader(req.Body).ReadToEndAsync();
                if (!string.IsNullOrEmpty(body))
                {
                    var entity = JsonConvert.DeserializeObject<SubscribeEntity>(body);

                    if (entity != null)
                    {
                        var result = await _context.Subscribers.FirstOrDefaultAsync(x => x.Email == entity.Email);

                        if (result != null)
                        {
                            _context.Entry(result).CurrentValues.SetValues(entity);
                            await _context.SaveChangesAsync();
                            return new OkResult();
                        }

                        _context.Subscribers.Add(entity);
                        await _context.SaveChangesAsync();
                        return new OkResult();
                    }
                }

                return new BadRequestResult();
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR : Run  :: {ex.Message}");

            }


            return new UnauthorizedResult();
        }
    }
}
