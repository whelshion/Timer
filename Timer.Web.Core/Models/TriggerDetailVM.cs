using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Quartz;
using Quartz.Spi;
using Quartz.Util;

namespace Timer.Web.Core.Models
{
    public class TriggerDetailVM
    {
        protected TriggerDetailVM(ITrigger trigger, ICalendar calendar)
        {
            Description = trigger.Description;
            TriggerType = trigger.GetType().AssemblyQualifiedNameWithoutVersion();
            Name = trigger.Key.Name;
            Group = trigger.Key.Group;
            CalendarName = trigger.CalendarName;
            Priority = trigger.Priority;
            StartTimeUtc = trigger.StartTimeUtc;
            EndTimeUtc = trigger.EndTimeUtc;
            NextFireTimes = TriggerUtils.ComputeFireTimes((IOperableTrigger)trigger, calendar, 10);
        }

        [Required(ErrorMessage = "�����������Ǳ�����")]
        [Display(Name = "����������")]
        public string SchedulerName { get; set; }
        [Required(ErrorMessage = "�����Ǳ�����")]
        [Display(Name = "����")]
        public string Name { get; set; }
        [Required(ErrorMessage = "�������Ǳ�����")]
        [Display(Name = "������")]
        public string Group { get; set; }
        [Required(ErrorMessage = "����������")]
        [Display(Name = "����������")]
        public string TriggerType { get; set; }
        [Display(Name = "˵��")]
        public string Description { get; set; }

        [Required(ErrorMessage = "���������Ǳ�����")]
        [Display(Name = "��������")]
        public string CalendarName { get; set; }
        [Required(ErrorMessage = "���ȼ��Ǳ�����")]
        [Display(Name = "���ȼ�")]
        public int Priority { get; set; }
        [Required(ErrorMessage = "��ʼ����ʱ���Ǳ�����")]
        [Display(Name = "��ʼʱ��")]
        public DateTimeOffset StartTimeUtc { get; set; }
        [Display(Name = "����ʱ��")]
        public DateTimeOffset? EndTimeUtc { get; set; }
        [Display(Name = "��������ʱ��")]
        public IReadOnlyList<DateTimeOffset> NextFireTimes { get; set; }

        public static TriggerDetailVM Create(ITrigger trigger, ICalendar calendar)
        {
            var simpleTrigger = trigger as ISimpleTrigger;
            if (simpleTrigger != null)
            {
                return new SimpleTriggerDetailVM(simpleTrigger, calendar);
            }
            var cronTrigger = trigger as ICronTrigger;
            if (cronTrigger != null)
            {
                return new CronTriggerDetailVM(cronTrigger, calendar);
            }
            var calendarIntervalTrigger = trigger as ICalendarIntervalTrigger;
            if (calendarIntervalTrigger != null)
            {
                return new CalendarIntervalTriggerDetailVM(calendarIntervalTrigger, calendar);
            }
            var dailyTimeIntervalTrigger = trigger as IDailyTimeIntervalTrigger;
            if (dailyTimeIntervalTrigger != null)
            {
                return new DailyTimeIntervalTriggerDetailVM(dailyTimeIntervalTrigger, calendar);
            }

            return new TriggerDetailVM(trigger, calendar);
        }


        public class CronTriggerDetailVM : TriggerDetailVM
        {
            public CronTriggerDetailVM(ICronTrigger trigger, ICalendar calendar) : base(trigger, calendar)
            {
                CronExpression = trigger.CronExpressionString;
                TimeZone = new TimeZoneVM(trigger.TimeZone);
            }

            public string CronExpression { get; set; }
            public TimeZoneVM TimeZone { get; set; }
        }

        public class SimpleTriggerDetailVM : TriggerDetailVM
        {
            public SimpleTriggerDetailVM(ISimpleTrigger trigger, ICalendar calendar) : base(trigger, calendar)
            {
                RepeatCount = trigger.RepeatCount;
                RepeatInterval = trigger.RepeatInterval;
                TimesTriggered = trigger.TimesTriggered;
            }

            public TimeSpan RepeatInterval { get; set; }
            public int RepeatCount { get; set; }
            public int TimesTriggered { get; set; }
        }

        public class CalendarIntervalTriggerDetailVM : TriggerDetailVM
        {
            public CalendarIntervalTriggerDetailVM(ICalendarIntervalTrigger trigger, ICalendar calendar) : base(trigger, calendar)
            {
                RepeatInterval = trigger.RepeatInterval;
                TimesTriggered = trigger.TimesTriggered;
                RepeatIntervalUnit = trigger.RepeatIntervalUnit;
                PreserveHourOfDayAcrossDaylightSavings = trigger.PreserveHourOfDayAcrossDaylightSavings;
                TimeZone = new TimeZoneVM(trigger.TimeZone);
                SkipDayIfHourDoesNotExist = trigger.SkipDayIfHourDoesNotExist;
            }

            public TimeZoneVM TimeZone { get; set; }
            public bool SkipDayIfHourDoesNotExist { get; set; }
            public bool PreserveHourOfDayAcrossDaylightSavings { get; set; }
            public IntervalUnit RepeatIntervalUnit { get; set; }
            public int RepeatInterval { get; set; }
            public int TimesTriggered { get; set; }
        }

        public class DailyTimeIntervalTriggerDetailVM : TriggerDetailVM
        {
            public DailyTimeIntervalTriggerDetailVM(IDailyTimeIntervalTrigger trigger, ICalendar calendar) : base(trigger, calendar)
            {
                RepeatInterval = trigger.RepeatInterval;
                TimesTriggered = trigger.TimesTriggered;
                RepeatIntervalUnit = trigger.RepeatIntervalUnit;
                TimeZone = new TimeZoneVM(trigger.TimeZone);
            }

            public TimeZoneVM TimeZone { get; set; }
            public IntervalUnit RepeatIntervalUnit { get; set; }
            public int TimesTriggered { get; set; }
            public int RepeatInterval { get; set; }
        }
    }
}