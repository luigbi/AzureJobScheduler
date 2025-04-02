# Quartz.NET Scheduler for Azure WebJobs

A .NET-based scheduler using Quartz.NET, designed to run as a continuous Azure WebJob. It fetches job definitions from a REST API and supports daily, weekly, or monthly triggers via CRON expressions.

## Features
- Dynamic schedule loading from a REST API
- CRON-based triggers (daily, weekly, monthly)
- Independent thread execution
- Azure Storage Queue integration

## Setup
1. Clone the repo: `git clone https://github.com/luigbi/AzureJobScheduler.git`
2. Configure `appsettings.json` with your REST API URL and credentials.
3. Build and deploy as an Azure WebJob.

## Learn More
Check out the full article on dev.to: https://dev.to/luigbi/job-scheduler-with-quartznet-and-azure-webjobs-2dne

## License
MIT License
