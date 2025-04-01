using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Quartz;
using Quartz.Logging;

namespace AzureJobScheduler
{
    class Program
    {
        static async Task Main()
        {
            var builder = new HostBuilder();

            SetConfiguration(builder);

            await InitializeScheduler(builder);

        }
        public static void SetConfiguration(HostBuilder builder)
        {
            builder.UseEnvironment(Environment.GetEnvironmentVariable("YOUR_ENVIRONMENT") ?? "development");

            builder.ConfigureWebJobs((context, b) =>
            {
                b.AddBuiltInBindings();
                b.AddEventHubs();
                b.AddTimers();
                b.AddAzureStorageCoreServices();
                //b.AddAzureStorageQueues();
                b.AddAzureStorageQueues(a =>
                {
                    a.BatchSize = 1;
                    a.NewBatchThreshold = 4;
                    a.VisibilityTimeout = TimeSpan.FromSeconds(1);
                    a.MaxDequeueCount = 4;
                    a.MaxPollingInterval = TimeSpan.FromSeconds(double.Parse(context.Configuration["YOUR_QUEUE_MAX_POLLING_INTERVAL"] ?? "60"));
                });
                b.AddAzureStorageBlobs();
            });
            builder.ConfigureLogging((context, b) =>
            {
                b.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Debug);
                b.AddConsole();
                // If the key exists in settings, use it to enable Application Insights.
                string? instrumentationKey = context.Configuration["APPINSIGHTS_INSTRUMENTATIONKEY"];
                if (!string.IsNullOrEmpty(instrumentationKey))
                {
                    b.AddApplicationInsightsWebJobs(o => o.InstrumentationKey = instrumentationKey);
                }
            });
            builder.ConfigureServices((cxt, services) =>
            {
                services.AddScoped<IRestAPIClient, RestAPIClient>();
                services.AddSingleton<ISchedulerListener, MySchedulerListener>();
                services.AddSingleton<IBackgroundJob, BackgroundJob>();
                services.AddQuartz(q =>
                {
                    q.UseMicrosoftDependencyInjectionJobFactory();
                    // these are the defaults
                    q.UseSimpleTypeLoader();
                    q.SchedulerName = "MyQuartzSchedulerInstance";
                    q.UseInMemoryStore();
                    q.UseDefaultThreadPool(tp =>
                    {
                        tp.MaxConcurrency = 10;
                    });
                });
                services.AddQuartzHostedService(opt =>
                {
                    opt.WaitForJobsToComplete = false;
                });
            });
        }

        public static async Task InitializeScheduler(HostBuilder builder)
        {
            var host = builder.Build();

            using (host)
            {
                var jobHost = host.Services.GetService(typeof(IJobHost)) as JobHost;
                var schedulerFactory = host.Services.GetRequiredService<ISchedulerFactory>();
                var loggerFactory = host.Services.GetRequiredService<ILoggerFactory>();
                var logger = host.Services.GetRequiredService<ILogger<Program>>();
                var scheduler = await schedulerFactory.GetScheduler();
                var restAPI = host.Services.GetRequiredService<IRestAPIClient>();
                var mySchedulerListener = host.Services.GetRequiredService<ISchedulerListener>();
                var myBackgroundJob = host.Services.GetRequiredService<IBackgroundJob>();
                try
                {

                    LogProvider.SetCurrentLogProvider(new QuartzLogProvider(logger));

                    Quartz.Logging.LogContext.SetCurrentLogProvider(loggerFactory);

                    logger.LogInformation($"LOAD SCHEDULE DEFS.....");

                    var schedules = await restAPI.FetchSchedulesAsync();

                    logger.LogInformation($"SCHEDULE DEFS LOADED");

                    scheduler.ListenerManager.AddSchedulerListener(mySchedulerListener);
                    await scheduler.Start();

                    foreach (var schedule in schedules)
                    {
                        await myBackgroundJob.AddOrUpdateSchedule(schedule);
                    }

                    await host.RunAsync();
                }
                catch (Exception ex)
                {
                    logger.LogError($"Error:{ex.Message} \n Source: {ex.Source}");

                    if (scheduler.IsStarted)
                        await scheduler.Shutdown();
                }
            }
        }
    }

    class QuartzLogProvider : ILogProvider
    {
        private static ILogger _logger;
        public QuartzLogProvider(ILogger logger)
        {
            _logger = logger;
        }
        public Logger GetLogger(string name)
        {
            return (level, func, exception, parameters) =>
            {
                if (level >= Quartz.Logging.LogLevel.Info && func != null)
                {
                    _logger.LogTrace("[" + DateTime.Now.ToLongTimeString() + "] [" + level + "] " + func(), parameters);
                    //Console.WriteLine("[" + DateTime.Now.ToLongTimeString() + "] [" + level + "] " + func(), parameters);
                }
                return true;
            };
        }

        public IDisposable OpenNestedContext(string message)
        {
            throw new NotImplementedException();
        }

        public IDisposable OpenMappedContext(string key, object value, bool destructure = false)
        {
            throw new NotImplementedException();
        }
    }
}

