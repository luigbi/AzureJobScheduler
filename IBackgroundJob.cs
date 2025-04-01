using Quartz;

namespace AzureJobScheduler
{
    /// <summary>
    /// Interface for background jobs.
    /// </summary>
    public interface IBackgroundJob : IJob
    {
        /// <summary>
        /// Adds or updates a job schedule.
        /// </summary>
        /// <param name="schedule">The job schedule.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task AddOrUpdateSchedule(JobScheduleDTO schedule);
    }
}