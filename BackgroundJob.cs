using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;

namespace AzureJobScheduler
{
    /// <summary>
    /// Represents a background job.
    /// </summary>
    public class BackgroundJob : IBackgroundJob
    {
        private readonly ILogger _logger;
        private IRestAPIClient _restAPI;
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="BackgroundJob"/> class.
        /// </summary>
        /// <param name="logger">The logger instance.</param>
        /// <param name="restAPI">The REST API client.</param>
        /// <param name="serviceProvider">The service provider.</param>
        public BackgroundJob(ILogger<BackgroundJob> logger, IRestAPIClient restAPI, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _restAPI = restAPI; // new RestAPIClient();
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Executes the job.
        /// </summary>
        /// <param name="context">The job execution context.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                var jobDataMap = context.MergedJobDataMap;

                var useJobDataMapConsoleOutput = jobDataMap.GetBoolean("UseJobDataMapConsoleOutput");

                _logger.LogInformation($"EXECUTE JOB: {context.JobDetail.Key.Name} - ID: {context.JobDetail.Key.Group}");

                //var _restAPI = new RestAPIClient();
                await _restAPI.RunJobs(int.Parse(context.JobDetail.Key.Name));

                if (useJobDataMapConsoleOutput)
                {
                    var consoleOutput = jobDataMap.GetString("ConsoleOutput");
                    await Console.Out.WriteLineAsync(context.Trigger.Key.ToString());
                }
                else
                {
                    await Console.Out.WriteLineAsync("Executing background job without JobDataMap");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"EXECUTE JOB: {context.JobDetail.Key.Name} - ID: {context.JobDetail.Key.Group} - MSG: " + ex.Message);
            }
        }

        /// <summary>
        /// Adds or updates a job schedule.
        /// </summary>
        /// <param name="schedule">The job schedule.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task AddOrUpdateSchedule(JobScheduleDTO schedule)
        {
            string cronSchedule = "0";
            var schedulerFactory = _serviceProvider.GetRequiredService<ISchedulerFactory>();
            var scheduler = await schedulerFactory.GetScheduler();

            try
            {
                if (schedule.RecurrenceType == 1) //daily
                {
                    cronSchedule += " " + schedule.StartTime.GetValueOrDefault().Minutes.ToString();
                    cronSchedule += " " + schedule.StartTime.GetValueOrDefault().Hours.ToString();
                    cronSchedule += " * * ?";

                    //if (schedule.Id == 17)
                    //    cronSchedule = "0 0/1 * 1/1 * ? *";//TEST EVERY MINUTE
                }
                else if (schedule.RecurrenceType == 2) //weekly
                {
                    string days = (schedule.Monday == true ? "MON" : "");
                    days += (schedule.Tuesday == true ? (days.Length > 1 ? ",TUE" : "TUE") : "");
                    days += (schedule.Wednesday == true ? (days.Length > 1 ? ",WED" : "WED") : "");
                    days += (schedule.Thursday == true ? (days.Length > 1 ? ",THU" : "THU") : "");
                    days += (schedule.Friday == true ? (days.Length > 1 ? ",FRI" : "FRI") : "");
                    days += (schedule.Saturday == true ? (days.Length > 1 ? ",SAT" : "SAT") : "");
                    days += (schedule.Sunday == true ? (days.Length > 1 ? ",SUN" : "SUN") : "");
                    cronSchedule += " " + schedule.StartTime.GetValueOrDefault().Minutes.ToString();
                    cronSchedule += " " + schedule.StartTime.GetValueOrDefault().Hours.ToString();
                    cronSchedule += " ? * " + days + " *";

                    //cronSchedule = "0 0/1 * 1/1 * ? *";//TEST EVERY 5 MINUTE
                }
                else if (schedule.RecurrenceType == 3) //monthly
                {
                    cronSchedule += " " + schedule.StartTime.GetValueOrDefault().Minutes.ToString();
                    cronSchedule += " " + schedule.StartTime.GetValueOrDefault().Hours.ToString();
                    cronSchedule += " " + schedule.DayOfMonth.ToString();
                    cronSchedule += " * ? *";

                    //cronSchedule = "0 0/10 * 1/1 * ? *";//TEST EVERY 10 MINUTE
                }

                if (schedule.RecurrenceType >= 1 && schedule.RecurrenceType <= 3)
                {

                    var job = JobBuilder.Create<BackgroundJob>()
                        .WithIdentity(name: schedule.Id.ToString() ?? "", group: "JOB_" + schedule.ClientId.ToString())
                        .UsingJobData("ConsoleOutput", "Executing background job using JobDataMap")
                        .UsingJobData("UseJobDataMapConsoleOutput", true)
                        .Build();

                    var trigger = TriggerBuilder.Create()
                        //.WithIdentity(name: "TRIGGER_" + schedule.Name ?? "", group: "TRIGGER_" + schedule.Id.ToString())
                        .WithIdentity(name: "TRIGGER_" + schedule.Id.ToString() ?? "", group: "TRIGGER_" + schedule.ClientId.ToString())
                        .StartAt(schedule.RecurrenceStartDate.GetValueOrDefault(DateTime.UtcNow))
                        .EndAt(schedule.RecurrenceEndDate.GetValueOrDefault(DateTime.UtcNow.AddYears(10)))
                        .WithCronSchedule(cronSchedule, x => x.InTimeZone(TimeZoneInfo.Utc))
                        .ForJob(job.Key)
                        .StartNow()
                    .Build();

                    if (await scheduler.CheckExists(job.Key))
                        if (schedule.RowStateId == 1)
                            await scheduler.RescheduleJob(trigger.Key, trigger);
                        else
                            await scheduler.DeleteJob(job.Key);
                    else if (schedule.RowStateId == 1)
                        await scheduler.ScheduleJob(job, trigger);

                    _logger.LogInformation($"LOAD JOB:{job.Key.Name} \n ID: {job.Key.Group} \n CRON: {cronSchedule}");
                }
                if (!scheduler.IsStarted)
                    await scheduler.Start();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error:{ex.Message} \n Source: {ex.Source}");
            }
        }
    }
}
