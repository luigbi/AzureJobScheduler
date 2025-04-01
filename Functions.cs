using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace AzureJobScheduler
{
    public static class Functions
    {
        [Singleton]
        [FunctionName("ProcessQueueMessage")]
        public static async Task ProcessQueueMessage([QueueTrigger("%YOUR_QUEUE_NAME%")] string myQueueItem, ILogger logger, IBinder binder)
        {
            try
            {
                // Deserialize the JSON response to a JobScheduleDTO object
                logger.LogInformation(myQueueItem);
                JobScheduleDTO jobScheduleDTO = JsonConvert.DeserializeObject<JobScheduleDTO>(myQueueItem);

                if (jobScheduleDTO != null)
                {
                    var builder = new HostBuilder();
                    Program.SetConfiguration(builder);
                    var host = builder.Build();
                    var myBackgroundJob = host.Services.GetRequiredService<IBackgroundJob>();
                    await myBackgroundJob.AddOrUpdateSchedule(jobScheduleDTO);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
            }
        }
    }
}
