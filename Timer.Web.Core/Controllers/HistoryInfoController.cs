using Microsoft.AspNetCore.Mvc;
using PetaPoco.NetCore;
using Timer.Web.Core.TableEntity;

namespace Timer.Web.Core.Controllers
{

    public class HistoryInfoController : BaseController
    {

        public Database _db;
        public HistoryInfoController(Database db)
        {
            _db = db;
        }

        public ActionResult Index(int take = 1000)
        {

            return View(_db.SkipTake<SingleUserAnalysisHistoryInfo>(0, take, "select * from single_user_analysis_history_info"));
        }

    }
}