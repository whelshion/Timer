using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Timer.MvcCore.Utils
{
    /// <summary>
    /// Internal helpers for working with tasks.
    /// </summary>
    internal static class TaskUtil
    {
        public static readonly Task CompletedTask = Task.FromResult(true);
    }
}
