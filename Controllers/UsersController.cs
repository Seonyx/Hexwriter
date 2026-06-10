using System;
using System.Linq;
using System.Web.Mvc;
using HexWriter.Web.Helpers;
using HexWriter.Web.Models;
using HexWriter.Web.Models.ViewModels.Admin;

namespace HexWriter.Web.Controllers
{
    [Authorize]
    public class UsersController : Controller
    {
        private HexWriterContext db = new HexWriterContext();

        private ActionResult RequireAdmin()
        {
            var user = AuthHelper.GetCurrentUser(HttpContext);
            if (user == null || !user.IsAdmin)
                return new HttpStatusCodeResult(403, "Admin access required");
            return null;
        }

        public ActionResult Index()
        {
            var check = RequireAdmin();
            if (check != null) return check;

            var users = db.Users
                .OrderBy(u => u.Username)
                .Select(u => new UserListItemViewModel
                {
                    Id = u.Id,
                    Username = u.Username,
                    Email = u.Email,
                    DisplayName = u.DisplayName,
                    Role = u.Role,
                    IsActive = u.IsActive,
                    CreatedAt = u.CreatedAt,
                    LastLoginAt = u.LastLoginAt
                })
                .ToList();

            return View(users);
        }

        public ActionResult Create()
        {
            var check = RequireAdmin();
            if (check != null) return check;

            return View(new CreateUserViewModel { Role = "Reviewer" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(CreateUserViewModel model)
        {
            var check = RequireAdmin();
            if (check != null) return check;

            if (!ModelState.IsValid)
                return View(model);

            if (db.Users.Any(u => u.Username == model.Username))
            {
                ModelState.AddModelError("Username", "Username already taken.");
                return View(model);
            }

            if (db.Users.Any(u => u.Email == model.Email))
            {
                ModelState.AddModelError("Email", "Email already in use.");
                return View(model);
            }

            db.Users.Add(new User
            {
                Username     = model.Username,
                Email        = model.Email,
                DisplayName  = model.DisplayName,
                Role         = model.Role,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
                IsActive     = true,
                CreatedAt    = DateTime.UtcNow
            });

            db.SaveChanges();
            TempData["Message"] = string.Format("User '{0}' created.", model.Username);
            return RedirectToAction("Index");
        }

        public ActionResult Edit(int id)
        {
            var check = RequireAdmin();
            if (check != null) return check;

            var user = db.Users.Find(id);
            if (user == null) return HttpNotFound();

            return View(new EditUserViewModel
            {
                Id          = user.Id,
                Username    = user.Username,
                Email       = user.Email,
                DisplayName = user.DisplayName,
                Role        = user.Role,
                IsActive    = user.IsActive
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(EditUserViewModel model)
        {
            var check = RequireAdmin();
            if (check != null) return check;

            if (!ModelState.IsValid)
                return View(model);

            var user = db.Users.Find(model.Id);
            if (user == null) return HttpNotFound();

            if (db.Users.Any(u => u.Email == model.Email && u.Id != model.Id))
            {
                ModelState.AddModelError("Email", "Email already in use.");
                return View(model);
            }

            user.DisplayName = model.DisplayName;
            user.Email       = model.Email;
            user.Role        = model.Role;
            user.IsActive    = model.IsActive;
            db.SaveChanges();

            TempData["Message"] = string.Format("User '{0}' updated.", user.Username);
            return RedirectToAction("Index");
        }

        public ActionResult ResetPassword(int id)
        {
            var check = RequireAdmin();
            if (check != null) return check;

            var user = db.Users.Find(id);
            if (user == null) return HttpNotFound();

            return View(new ResetPasswordViewModel { Id = user.Id, Username = user.Username });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ResetPassword(ResetPasswordViewModel model)
        {
            var check = RequireAdmin();
            if (check != null) return check;

            if (!ModelState.IsValid)
                return View(model);

            var user = db.Users.Find(model.Id);
            if (user == null) return HttpNotFound();

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
            db.SaveChanges();

            TempData["Message"] = string.Format("Password reset for '{0}'.", user.Username);
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
