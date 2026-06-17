using System.Web.Mvc;

namespace HexWriter.Web.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Title = "Hexwriter - Long-form book editing for collaborative authors";
            ViewBag.MetaDescription = "Hexwriter is a focused book editor for authors and editorial teams. Import manuscripts, edit by chapter, compare drafts, and export your finished work.";
            return View();
        }
    }
}
