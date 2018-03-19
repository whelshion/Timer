﻿using Quartz;

namespace Timer.Web.Core.Controllers.Dto
{
    public class KeyDto
    {
        public KeyDto(JobKey jobKey)
        {
            Name = jobKey.Name;
            Group = jobKey.Group;
        }

        public KeyDto(TriggerKey triggerKey)
        {
            Name = triggerKey.Name;
            Group = triggerKey.Group;
        }

        public string Name { get; private set; }
        public string Group { get; private set; }
    }
}