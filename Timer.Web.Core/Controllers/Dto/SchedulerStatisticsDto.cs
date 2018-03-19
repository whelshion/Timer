using Quartz;

namespace Timer.Web.Core.Controllers.Dto
{
    public class SchedulerStatisticsDto
    {
        public SchedulerStatisticsDto(SchedulerMetaData metaData)
        {
            NumberOfJobsExecuted = metaData.NumberOfJobsExecuted;
        }

        public int NumberOfJobsExecuted { get; private set; }
    }
}