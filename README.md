# Job Scheduler with Quartz.NET and Azure WebJobs

This article explores a .NET-based job scheduler built using the Quartz.NET library and hosted as a continuous Azure WebJob. The scheduler dynamically retrieves job definitions from a database via a REST API, loads them into a shared Quartz scheduler, and executes each schedule independently in its own thread. Schedules can be triggered daily, weekly, or monthly using CRON expressions, making it a flexible and scalable solution for automated task management in the cloud.

* * *

## Introduction

In modern cloud-based applications, scheduling recurring tasks is a common requirement. Whether it's running daily reports, weekly data syncs, or monthly maintenance jobs, a reliable scheduler is essential. This implementation leverages Quartz.NET, a powerful open-source scheduling library, and integrates it with Azure WebJobs for seamless deployment and execution in the Azure cloud. The scheduler fetches job definitions from a database through a REST API, allowing for dynamic configuration without redeploying the application. Each schedule runs in its own thread, ensuring independent execution, and can be triggered based on daily, weekly, or monthly CRON patterns.

* * *

## Code Structure and Key Components

The application is composed of several key components, each responsible for a specific aspect of the scheduling process. Below is a breakdown of the main parts:

## 1. Main Entry Point (Program.cs)

The application starts in Program.cs, where the host is configured, and the Quartz scheduler is initialized.

### Key Methods:

*   `SetConfiguration(HostBuilder builder)`  
    Configures the hosting environment, WebJobs extensions, logging, and dependency injection.
    
    *   Environment: Sets the environment (e.g., "development") based on the ALICE_ENVIRONMENT variable.
        
    *   WebJobs Configuration: Adds support for EventHubs, Timers, Azure Storage Queues, and Blobs.
        
        
        
            builder.ConfigureWebJobs((context, b) =>
            {
                b.AddEventHubs();
                b.AddTimers();
                b.AddAzureStorageQueues(a => { a.BatchSize = 1; /* ... */ });
                b.AddAzureStorageBlobs();
            });
        
    *   Logging: Configures logging with a minimum level of Debug and console output, with optional Application Insights integration.
        
    *   Services: Registers services like IRestAPIClient, ISchedulerListener, and IBackgroundJob using .NET Core’s dependency injection.
        
    *   Quartz Setup: Configures Quartz with an in-memory store, a thread pool (max concurrency of 10), and a scheduler name ("AliceScheduler").
        
        
        
            services.AddQuartz(q =>
            {
                q.SchedulerName = "AliceScheduler";
                q.UseInMemoryStore();
                q.UseDefaultThreadPool(tp => { tp.MaxConcurrency = 10; });
            });
        
*   `InitializeScheduler(HostBuilder builder)`  
    Builds the host, retrieves services, and starts the scheduler.
    
    *   Fetches schedules from the REST API using IRestAPIClient.
        
    *   Adds a scheduler listener (ISchedulerListener) and starts the Quartz scheduler.
        
    *   Iterates through the schedules and calls AddOrUpdateSchedule to load them into the scheduler.
        
       
        
            var schedules = await restAPI.FetchSchedulesAsync();
            foreach (var schedule in schedules)
            {
                await myBackgroundJob.AddOrUpdateSchedule(schedule);
            }
            await host.RunAsync();
        

## 2. REST API Client (RestAPIClient.cs)

The RestAPIClient class handles communication with the REST API to fetch job schedules and execute jobs.

###Key Methods:

*   `Authenticate()`  
    Authenticates with the API using a username and password from the configuration, returning a bearer token.
    
   
    
        string jsonRequestBody = JsonConvert.SerializeObject(new Dictionary<string, string>
        {
            { "UserName", _config["ALICE_RESTAPI_USERNAME"] },
            { "Password", _config["ALICE_RESTAPI_PASSWORD"] }
        });
    
*   `FetchSchedulesAsync()`  
    Retrieves a list of JobScheduleDTO objects from the API using the bearer token.
    
    
    
        HttpResponseMessage getResponse = await client.GetAsync(endpoint);
        List<JobScheduleDTO> response = JsonConvert.DeserializeObject<List<JobScheduleDTO>>(responseBody);
    
*   `RunJobs(long scheduleId)`  
    Triggers the execution of jobs for a specific schedule via a POST request to the API.
    

## 3. Scheduler Listener (MySchedulerListener.cs)

The MySchedulerListener class implements Quartz’s ISchedulerListener interface to monitor and log scheduler events (e.g., job added, paused, or errors).

*   Each method logs the event using the injected ILogger.
    
    
    
        public Task JobAdded(IJobDetail jobDetail, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation($"JobAdded:{jobDetail.Key.Name} \n ID: {jobDetail.Key.Group}");
            return Task.CompletedTask;
        }
    

## 4. Background Job (BackgroundJob.cs)

The BackgroundJob class implements IBackgroundJob and contains the logic for executing jobs and managing schedules.
Processing 1 inserted images...

### Key Methods:

*   `Execute(IJobExecutionContext context)`  
    Executes a job when triggered by Quartz.
    
    *   Retrieves job data from the context and logs the execution.
        
    *   Calls the REST API to run jobs for the schedule.
        
    
    code
    
        await _restAPI.RunJobs(int.Parse(context.JobDetail.Key.Name));
    
*   `AddOrUpdateSchedule(JobScheduleDTO schedule)`  
    Adds or updates a schedule in the Quartz scheduler.
    
    *   CRON Expression Generation: Constructs a CRON expression based on the schedule’s recurrence type:
        
        *   Daily: `0 [Minutes] [Hours] * * ?`
            
        *   Weekly: `0 [Minutes] [Hours] ? * [Days] * (e.g., MON,TUE).`
            
        *   Monthly: `0 [Minutes] [Hours] [DayOfMonth] * ? *.`
            
        
        code
        
            if (schedule.RecurrenceType == 1) // Daily
            {
                cronSchedule += " " + schedule.StartTime.GetValueOrDefault().Minutes.ToString();
                cronSchedule += " " + schedule.StartTime.GetValueOrDefault().Hours.ToString();
                cronSchedule += " * * ?";
            }
        
    *   Job and Trigger Creation: Creates a Quartz job and trigger with the CRON expression.
        
  
        
            var job = JobBuilder.Create<BackgroundJob>()
                .WithIdentity(name: schedule.Id.ToString(), group: "JOB_" + schedule.ClientId.ToString())
                .Build();
            
            var trigger = TriggerBuilder.Create()
                .WithIdentity(name: "TRIGGER_" + schedule.Id.ToString(), group: "TRIGGER_" + schedule.ClientId.ToString())
                .WithCronSchedule(cronSchedule, x => x.InTimeZone(TimeZoneInfo.Utc))
                .StartNow()
                .Build();
        
    *   Scheduling Logic: Reschedules, schedules, or deletes the job based on its RowStateId.
        

## 5. Queue Trigger (Functions.cs)

The `Functions` class contains a static method to handle dynamic schedule updates via Azure Storage Queues.

*   `ProcessQueueMessage`  
    Triggered by a queue message, deserializes it into a `JobScheduleDTO`, and calls `AddOrUpdateSchedule`.
    

    
        JobScheduleDTO jobScheduleDTO = JsonConvert.DeserializeObject<JobScheduleDTO>(myQueueItem);
        await myBackgroundJob.AddOrUpdateSchedule(jobScheduleDTO);
    

* * *

# How It Works


1.  ### Startup:
    
    *   The application starts in `Main()`, configures the host, and initializes the scheduler.
        
    *   Services like `IRestAPIClient` and `IBackgroundJob` are registered via dependency injection.
        
2.  ### Schedule Loading:
    
    *   The `RestAPIClient` fetches job schedules from the REST API.
        
    *   Each schedule is passed to `AddOrUpdateSchedule`, which generates a CRON expression and creates a Quartz job and trigger.
        
3.  ### Execution:
    
    *   Quartz triggers jobs based on their CRON schedules.
        
    *   The `BackgroundJob.Execute` method runs, invoking the REST API to execute the jobs.
        
4.  ### Dynamic Updates:
    
    *   Queue messages trigger the `ProcessQueueMessage` function, allowing real-time schedule updates.
        
5.  ### Concurrency:
    
    *   Each schedule runs in its own thread, managed by Quartz’s thread pool (max 10 concurrent threads).
        

* * *

# Key Features

*   Azure WebJobs: Ensures the scheduler runs continuously in Azure, providing reliability and scalability.
    
*   Quartz.NET: Offers robust scheduling capabilities with CRON-based triggers for daily, weekly, or monthly jobs.
    
*   REST API Integration: Dynamically fetches schedules from a database, decoupling configuration from code.
    
*   CRON Expressions: Flexibly defines recurrence patterns based on schedule properties.
    
*   Threading: Independent execution of schedules in separate threads, controlled by Quartz.
    

* * *

# Conclusion

This implementation demonstrates a scalable and maintainable scheduler built with Quartz.NET and hosted as an Azure WebJob. By integrating with a REST API for dynamic schedule management and supporting various recurrence patterns via CRON expressions, it provides a flexible solution for automated job scheduling in the cloud. The use of Azure Storage Queues for real-time updates and comprehensive logging further enhances its adaptability and observability, making it a solid choice for managing recurring tasks in a distributed environment.

* * *
