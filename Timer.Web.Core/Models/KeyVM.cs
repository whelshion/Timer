using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Timer.Web.Core.Models
{
    public class KeyVM
    {
        public KeyVM(JobKey jobKey)
        {
            Name = jobKey.Name;
            Group = jobKey.Group;
        }

        public KeyVM(TriggerKey triggerKey)
        {
            Name = triggerKey.Name;
            Group = triggerKey.Group;
        }

        public string Name { get; private set; }
        public string Group { get; private set; }
    }
}
