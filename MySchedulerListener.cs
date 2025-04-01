using Microsoft.Extensions.Logging;
using Quartz;

namespace AzureJobScheduler
{
    internal class MySchedulerListener : ISchedulerListener
    {
        private readonly ILogger _logger;

        public MySchedulerListener(ILogger<MySchedulerListener> logger)
        {
            _logger = logger;
        }

        public Task JobAdded(IJobDetail jobDetail, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation($"JobAdded:{jobDetail.Key.Name} \n ID: {jobDetail.Key.Group}");
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error:{ex.Message} \n Source: {ex.Source}");
                return Task.CompletedTask;
            }
        }

        public Task JobDeleted(JobKey jobKey, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation($"JobDeleted:{jobKey.Name} \n ID: {jobKey.Group}");
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error:{ex.Message} \n Source: {ex.Source}");
                return Task.CompletedTask;
            }
        }

        public Task JobInterrupted(JobKey jobKey, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation($"JobInterrupted:{jobKey.Name} \n ID: {jobKey.Group}");
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error:{ex.Message} \n Source: {ex.Source}");

                return Task.CompletedTask;
            }
        }

        public Task JobPaused(JobKey jobKey, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation($"JobPaused:{jobKey.Name} \n ID: {jobKey.Group}");
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error:{ex.Message} \n Source: {ex.Source}");

                return Task.CompletedTask;
            }
        }

        public Task JobResumed(JobKey jobKey, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation($"JobResumed:{jobKey.Name} \n ID: {jobKey.Group}");
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error:{ex.Message} \n Source: {ex.Source}");

                return Task.CompletedTask;
            }
        }

        public Task JobScheduled(ITrigger trigger, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation($"JobScheduled:{trigger.Key.Name} \n ID:{trigger.Key.Group}");
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error:{ex.Message} \n Source: {ex.Source}");

                return Task.CompletedTask;
            }
        }

        public Task JobsPaused(string jobGroup, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation($"JobsPaused:{jobGroup?.FirstOrDefault().ToString()}");
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error:{ex.Message} \n Source: {ex.Source}");

                return Task.CompletedTask;
            }
        }

        public Task JobsResumed(string jobGroup, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation($"JobsResumed:{jobGroup}");
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error:{ex.Message} \n Source: {ex.Source}");

                return Task.CompletedTask;
            }
        }

        public Task JobUnscheduled(TriggerKey triggerKey, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation($"JobUnscheduled:{triggerKey.Name} \n ID:{triggerKey.Group}");
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error:{ex.Message} \n Source: {ex.Source}");

                return Task.CompletedTask;
            }
        }

        public Task SchedulerError(string msg, SchedulerException cause, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogError($"SchedulerError:{msg} \n cause:{cause}");
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error:{ex.Message} \n Source: {ex.Source}");

                return Task.CompletedTask;
            }
        }

        public Task SchedulerInStandbyMode(CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation($"SchedulerInStandbyMode");
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error:{ex.Message} \n Source: {ex.Source}");

                return Task.CompletedTask;
            }
        }

        public Task SchedulerShutdown(CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation($"SchedulerShutdown");
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error:{ex.Message} \n Source: {ex.Source}");

                return Task.CompletedTask;
            }
        }

        public Task SchedulerShuttingdown(CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation($"SchedulerShuttingdown");
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error:{ex.Message} \n Source: {ex.Source}");

                return Task.CompletedTask;
            }
        }

        public Task SchedulerStarted(CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation($"SchedulerStarted");
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error:{ex.Message} \n Source: {ex.Source}");

                return Task.CompletedTask;
            }
        }

        public Task SchedulerStarting(CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation($"SchedulerStarting");
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error:{ex.Message} \n Source: {ex.Source}");

                return Task.CompletedTask;
            }
        }

        public Task SchedulingDataCleared(CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation($"SchedulingDataCleared");
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error:{ex.Message} \n Source: {ex.Source}");

                return Task.CompletedTask;
            }
        }

        public Task TriggerFinalized(ITrigger trigger, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogError($"TriggerFinalized:{trigger.Key.Name} \n ID:{trigger.Key.Group}");
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error:{ex.Message} \n Source: {ex.Source}");

                return Task.CompletedTask;
            }
        }

        public Task TriggerPaused(TriggerKey triggerKey, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogError($"TriggerPaused:{triggerKey.Name} \n ID:{triggerKey.Group}");
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error:{ex.Message} \n Source: {ex.Source}");

                return Task.CompletedTask;
            }
        }

        public Task TriggerResumed(TriggerKey triggerKey, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogError($"TriggerResumed:{triggerKey.Name} \n ID:{triggerKey.Group}");
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error:{ex.Message} \n Source: {ex.Source}");

                return Task.CompletedTask;
            }
        }

        public Task TriggersPaused(string? triggerGroup, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogError($"TriggersPaused:{triggerGroup?.FirstOrDefault().ToString()}");
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error:{ex.Message} \n Source: {ex.Source}");

                return Task.CompletedTask;
            }
        }

        public Task TriggersResumed(string? triggerGroup, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogError($"TriggersResumed:{triggerGroup?.FirstOrDefault().ToString()}");
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error:{ex.Message} \n Source: {ex.Source}");

                return Task.CompletedTask;
            }
        }
    }
}
