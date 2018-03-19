using System.Collections.Generic;

namespace Timer.Web.Core.Controllers.Dto
{
    public class JobHistoryViewModel
    {
        public JobHistoryViewModel(IReadOnlyList<JobHistoryEntryDto> entries, string errorMessage)
        {
            HistoryEntries = entries;
            ErrorMessage = errorMessage;
        }

        public IReadOnlyList<JobHistoryEntryDto> HistoryEntries { get; }
        public string ErrorMessage { get; }
    }
}