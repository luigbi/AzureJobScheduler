using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace AzureJobScheduler
{
    public interface ISchedulerListener
    {
        ValueTask JobScheduled(ITrigger trigger);

        ValueTask JobUnscheduled(string triggerName, string triggerGroup);

        ValueTask TriggerFinalized(ITrigger trigger);

        ValueTask TriggersPaused(string triggerName, string triggerGroup);

        ValueTask TriggersResumed(string triggerName, string triggerGroup);

        ValueTask JobsPaused(string jobName, string jobGroup);

        ValueTask JobsResumed(string jobName, string jobGroup);

        ValueTask SchedulerError(string msg, SchedulerException cause);

        ValueTask SchedulerShutdown();
    }
}
