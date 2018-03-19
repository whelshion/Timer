using System;

namespace Timer.Web.Core.Models
{
    public class TimeZoneVM
    {
        public TimeZoneVM(TimeZoneInfo timeZone)
        {
            Id = timeZone.Id;
            StandardName = timeZone.StandardName;
            DisplayName = timeZone.DisplayName;
        }

        public string Id { get; set; }
        public string StandardName { get; set; }
        public string DisplayName { get; set; }
    }
}