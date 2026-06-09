using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using HexWriter.Web.Models;
using HexWriter.Web.Models.ViewModels.Account;

namespace HexWriter.Web.Controllers
{
    public class AccountController : Controller
    {
        private HexWriterContext db = new HexWriterContext();

        public ActionResult Login()
        {
            if (User.Identity.IsAuthenticated)
                return RedirectToAction("Dashboard", "Admin");

            return View(new LoginViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = db.Users.FirstOrDefault(u => u.Username == model.Username && u.IsActive);

            if (user == null || !BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash))
            {
                ModelState.AddModelError("", "Invalid username or password.");
                return View(model);
            }

            user.LastLoginAt = DateTime.UtcNow;
            db.SaveChanges();

            var ticket = new FormsAuthenticationTicket(
                version: 1,
                name: user.Username,
                issueDate: DateTime.UtcNow,
                expiration: DateTime.UtcNow.AddMinutes(120),
                isPersistent: false,
                userData: string.Format("{0}|{1}|{2}", user.Id, user.Username, user.Role)
            );

            var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, FormsAuthentication.Encrypt(ticket));
            Response.Cookies.Add(cookie);

            var returnUrl = Request.QueryString["ReturnUrl"];
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Dashboard", "Admin");
        }

        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Login");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                db.Dispose();
            base.Dispose(disposing);
        }
    }
}
