using Microsoft.Azure.WebJobs.Extensions.Timers;

namespace AzureJobScheduler
{
    public class MyWeeklySchedule : WeeklySchedule
    {
        public MyWeeklySchedule()
        {
            // Every Monday at 8 AM
            Add(DayOfWeek.Monday, new TimeSpan(8, 0, 0));

            // Twice on Wednesdays at 9:30 AM and 9:00 PM
            Add(DayOfWeek.Wednesday, new TimeSpan(9, 30, 0));
            Add(DayOfWeek.Wednesday, new TimeSpan(21, 30, 0));

            // Every Friday at 10:00 AM
            Add(DayOfWeek.Friday, new TimeSpan(10, 0, 0));
        }
    }

    public class MyDailySchedule : DailySchedule
    {
        public MyDailySchedule()
            : base("8:00:00", "12:00:00", "22:00:00")
        {
        }
    }
}