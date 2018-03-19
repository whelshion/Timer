using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using Timer.Web.Core.Utils;

namespace Timer.Web.Core.Models
{
    public class ServerDetailVM
    {
        public ServerDetailVM(IEnumerable<IScheduler> schedulers)
        {
            Name = Environment.MachineName;
            Address = "localhost";
            Schedulers = schedulers.Select(x => x.SchedulerName).ToList();
        }

        public string Name { get; set; }
        public string Address { get; set; }
        public IReadOnlyList<string> Schedulers { get; set; } 
    }

    public class ServerHeaderVM
    {
        public ServerHeaderVM(Server server)
        {
            Name = server.Name;
        }

        public string Name { get; set; }
        public string Address { get; set; }
    }

}