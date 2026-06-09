using System;
using System.Linq;
using System.Web.Mvc;
using HexWriter.Web.Models;
using HexWriter.Web.Models.ViewModels.Admin;

namespace HexWriter.Web.Controllers
{
    [Authorize]
    public class AdminController : Controller
    {
        private HexWriterContext db = new HexWriterContext();

        // ==================== Dashboard ====================

        public ActionResult Dashboard()
        {

            var model = new AdminDashboardViewModel
            {
                TotalPages = db.Pages.Count(),
                TotalAuthors = db.Authors.Count(),
                TotalBooks = db.Books.Count(),
                TotalDivisions = db.Divisions.Count(),
                UnreadSubmissions = db.ContactSubmissions.Count(s => !s.IsRead)
            };

            return View(model);
        }

        // ==================== Pages ====================

        public ActionResult Pages()
        {

            var pages = db.Pages.OrderBy(p => p.SortOrder).ToList();
            return View("Pages/Index", pages);
        }

        public ActionResult CreatePage()
        {

            var model = new AdminPageViewModel();
            PopulatePageDropdowns(model);
            return View("Pages/Edit", model);
        }

        public ActionResult EditPage(int id)
        {

            var page = db.Pages.Find(id);
            if (page == null) return HttpNotFound();

            var model = new AdminPageViewModel
            {
                PageId = page.PageId,
                Slug = page.Slug,
                Title = page.Title,
                MetaDescription = page.MetaDescription,
                MetaKeywords = page.MetaKeywords,
                Content = page.Content,
                ParentPageId = page.ParentPageId,
                DivisionId = page.DivisionId,
                SortOrder = page.SortOrder,
                IsPublished = page.IsPublished,
                ShowInNavigation = page.ShowInNavigation
            };

            PopulatePageDropdowns(model);
            return View("Pages/Edit", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SavePage(AdminPageViewModel model)
        {

            if (!ModelState.IsValid)
            {
                PopulatePageDropdowns(model);
                return View("Pages/Edit", model);
            }

            Page page;
            if (model.PageId > 0)
            {
                page = db.Pages.Find(model.PageId);
                if (page == null) return HttpNotFound();
            }
            else
            {
                page = new Page { CreatedDate = DateTime.Now };
                db.Pages.Add(page);
            }

            page.Slug = model.Slug;
            page.Title = model.Title;
            page.MetaDescription = model.MetaDescription;
            page.MetaKeywords = model.MetaKeywords;
            page.Content = model.Content;
            page.ParentPageId = model.ParentPageId;
            page.DivisionId = model.DivisionId;
            page.SortOrder = model.SortOrder;
            page.IsPublished = model.IsPublished;
            page.ShowInNavigation = model.ShowInNavigation;
            page.ModifiedDate = DateTime.Now;

            db.SaveChanges();
            return RedirectToAction("Pages");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeletePage(int id)
        {

            var page = db.Pages.Find(id);
            if (page != null)
            {
                db.Pages.Remove(page);
                db.SaveChanges();
            }

            return RedirectToAction("Pages");
        }

        // ==================== Authors ====================

        public ActionResult Authors()
        {

            var authors = db.Authors.OrderBy(a => a.SortOrder).ToList();
            return View("Authors/Index", authors);
        }

        public ActionResult CreateAuthor()
        {
            return View("Authors/Edit", new Author());
        }

        public ActionResult EditAuthor(int id)
        {

            var author = db.Authors.Find(id);
            if (author == null) return HttpNotFound();

            return View("Authors/Edit", author);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveAuthor(Author model)
        {

            if (!ModelState.IsValid)
                return View("Authors/Edit", model);

            if (model.AuthorId > 0)
            {
                var author = db.Authors.Find(model.AuthorId);
                if (author == null) return HttpNotFound();

                author.PenName = model.PenName;
                author.Biography = model.Biography;
                author.PhotoUrl = model.PhotoUrl;
                author.Genre = model.Genre;
                author.Website = model.Website;
                author.SortOrder = model.SortOrder;
                author.IsActive = model.IsActive;
            }
            else
            {
                model.CreatedDate = DateTime.Now;
                db.Authors.Add(model);
            }

            db.SaveChanges();
            return RedirectToAction("Authors");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteAuthor(int id)
        {

            var author = db.Authors.Find(id);
            if (author != null)
            {
                db.Authors.Remove(author);
                db.SaveChanges();
            }

            return RedirectToAction("Authors");
        }

        // ==================== Books ====================

        public ActionResult Books()
        {

            var books = db.Books.OrderBy(b => b.AuthorId).ThenBy(b => b.SortOrder).ToList();
            ViewBag.Authors = db.Authors.OrderBy(a => a.SortOrder).ToList();
            return View("Books/Index", books);
        }

        public ActionResult CreateBook()
        {

            ViewBag.Authors = new SelectList(
                db.Authors.OrderBy(a => a.PenName).ToList(),
                "AuthorId", "PenName");

            return View("Books/Edit", new Book());
        }

        public ActionResult EditBook(int id)
        {

            var book = db.Books.Find(id);
            if (book == null) return HttpNotFound();

            ViewBag.Authors = new SelectList(
                db.Authors.OrderBy(a => a.PenName).ToList(),
                "AuthorId", "PenName", book.AuthorId);

            return View("Books/Edit", book);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveBook(Book model)
        {

            if (!ModelState.IsValid)
            {
                ViewBag.Authors = new SelectList(
                    db.Authors.OrderBy(a => a.PenName).ToList(),
                    "AuthorId", "PenName", model.AuthorId);
                return View("Books/Edit", model);
            }

            if (model.BookId > 0)
            {
                var book = db.Books.Find(model.BookId);
                if (book == null) return HttpNotFound();

                book.AuthorId = model.AuthorId;
                book.Title = model.Title;
                book.Synopsis = model.Synopsis;
                book.CoverImageUrl = model.CoverImageUrl;
                book.AmazonUrl = model.AmazonUrl;
                book.KDPUrl = model.KDPUrl;
                book.ISBN = model.ISBN;
                book.PublicationDate = model.PublicationDate;
                book.Genre = model.Genre;
                book.SortOrder = model.SortOrder;
                book.IsPublished = model.IsPublished;
            }
            else
            {
                model.CreatedDate = DateTime.Now;
                db.Books.Add(model);
            }

            db.SaveChanges();
            return RedirectToAction("Books");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteBook(int id)
        {

            var book = db.Books.Find(id);
            if (book != null)
            {
                db.Books.Remove(book);
                db.SaveChanges();
            }

            return RedirectToAction("Books");
        }

        // ==================== Content Blocks ====================

        public ActionResult ContentBlocks()
        {

            var blocks = db.ContentBlocks.OrderBy(b => b.BlockKey).ToList();
            return View("ContentBlocks/Index", blocks);
        }

        public ActionResult EditContentBlock(int id)
        {

            var block = db.ContentBlocks.Find(id);
            if (block == null) return HttpNotFound();

            return View("ContentBlocks/Edit", block);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult SaveContentBlock(ContentBlock model)
        {

            if (!ModelState.IsValid)
                return View("ContentBlocks/Edit", model);

            var block = db.ContentBlocks.Find(model.BlockId);
            if (block == null) return HttpNotFound();

            block.Title = model.Title;
            block.Content = model.Content;
            block.IsActive = model.IsActive;
            block.ModifiedDate = DateTime.Now;

            db.SaveChanges();
            return RedirectToAction("ContentBlocks");
        }

        // ==================== Contact Submissions ====================

        public ActionResult Submissions()
        {

            var submissions = db.ContactSubmissions
                .OrderByDescending(s => s.SubmittedDate)
                .ToList();

            return View("ContactSubmissions/Index", submissions);
        }

        public ActionResult ViewSubmission(int id)
        {

            var submission = db.ContactSubmissions.Find(id);
            if (submission == null) return HttpNotFound();

            if (!submission.IsRead)
            {
                submission.IsRead = true;
                db.SaveChanges();
            }

            return View("ContactSubmissions/View", submission);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteSubmission(int id)
        {

            var submission = db.ContactSubmissions.Find(id);
            if (submission != null)
            {
                db.ContactSubmissions.Remove(submission);
                db.SaveChanges();
            }

            return RedirectToAction("Submissions");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult MarkAsSpam(int id)
        {

            var submission = db.ContactSubmissions.Find(id);
            if (submission != null)
            {
                submission.IsSpam = true;
                db.SaveChanges();
            }

            return RedirectToAction("Submissions");
        }

        // ==================== Settings ====================

        public ActionResult Settings()
        {

            var settings = db.SiteSettings.OrderBy(s => s.SettingKey).ToList();
            return View("Settings/Index", settings);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveSettings(FormCollection form)
        {

            var settings = db.SiteSettings.ToList();
            foreach (var setting in settings)
            {
                var newValue = form[setting.SettingKey];
                if (newValue != null && newValue != setting.SettingValue)
                {
                    setting.SettingValue = newValue;
                    setting.ModifiedDate = DateTime.Now;
                }
            }

            db.SaveChanges();
            TempData["Message"] = "Settings saved successfully.";
            return RedirectToAction("Settings");
        }

        // ==================== Helpers ====================

        private void PopulatePageDropdowns(AdminPageViewModel model)
        {
            model.ParentPages = db.Pages
                .Where(p => p.PageId != model.PageId)
                .OrderBy(p => p.Title)
                .Select(p => new SelectListItem
                {
                    Value = p.PageId.ToString(),
                    Text = p.Title
                })
                .ToList();

            model.ParentPages.Insert(0, new SelectListItem { Value = "", Text = "(No Parent)" });

            model.Divisions = db.Divisions
                .OrderBy(d => d.SortOrder)
                .Select(d => new SelectListItem
                {
                    Value = d.DivisionId.ToString(),
                    Text = d.Name
                })
                .ToList();

            model.Divisions.Insert(0, new SelectListItem { Value = "", Text = "(No Division)" });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
