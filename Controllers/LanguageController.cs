using System;
using System.Web;
using System.Web.Mvc;

namespace HexWriter.Web.Controllers
{
    public class LanguageController : Controller
    {
        private static readonly string[] SupportedLanguages = { "en", "es" };

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Set(string lang, string returnUrl)
        {
            if (!Array.Exists(SupportedLanguages, l => l == lang))
                lang = "en";

            Response.Cookies.Add(new HttpCookie("hw_lang", lang)
            {
                HttpOnly = true,
                Expires  = DateTime.UtcNow.AddYears(1)
            });

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return Redirect(Request.UrlReferrer != null ? Request.UrlReferrer.PathAndQuery : "/");
        }
    }
}
