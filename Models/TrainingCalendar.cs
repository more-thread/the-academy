using Microsoft.EntityFrameworkCore.Metadata.Conventions;

namespace TRS.Models
{
    public class CalendarActivity : ISchedulerEvent
    {
        public string ID { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;        
        public string Description { get; set; } = string.Empty;
        public bool IsAllDay { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public string StartTimezone { get; set; } = string.Empty;
        public string EndTimezone { get; set; } = string.Empty;
        public string RecurrenceRule { get; set; } = string.Empty;
        public string RecurrenceException { get; set; } = string.Empty;
        public string TrainingScheduleStatus { get; set; } = string.Empty;
        public TrainingSchedule? Schedule { get; set; }
    }    
}
