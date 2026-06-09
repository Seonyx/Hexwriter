using System.Linq;
using System.Web.Mvc;
using HexWriter.Web.Models;

namespace HexWriter.Web.Controllers
{
    [Authorize]
    public class ImportLogController : Controller
    {
        private HexWriterContext db = new HexWriterContext();

        public ActionResult Index(int projectId)
        {

            var project = db.BookProjects.Find(projectId);
            if (project == null) return HttpNotFound();

            ViewBag.ProjectId   = projectId;
            ViewBag.ProjectName = project.ProjectName;

            var logs = db.ImportLogs
                .Where(l => l.BookProjectID == projectId)
                .OrderByDescending(l => l.ImportedAt)
                .ToList();

            return View(logs);
        }

        public ActionResult Detail(int id)
        {

            var log = db.ImportLogs.Find(id);
            if (log == null) return HttpNotFound();

            ViewBag.ProjectName = db.BookProjects.Find(log.BookProjectID)?.ProjectName ?? "";

            return View(log);
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
