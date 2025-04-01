namespace AzureJobScheduler
{
    public interface IRestAPIClient
    {
        Task<string> Authenticate();
        Task FetchJob(long jobId);
        Task<List<JobScheduleDTO>> FetchSchedulesAsync(int clientId);
        Task<List<JobScheduleDTO>> FetchSchedulesAsync();
        Task RunJobs(long scheduleId);
    }
}