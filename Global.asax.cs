using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using System.Web.Routing;
using HexWriter.Web.Models;

namespace HexWriter.Web
{
    public class MvcApplication : HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            AntiForgeryConfig.SuppressIdentityHeuristicChecks = true;
            SeedAdminUser();
        }

        private static void SeedAdminUser()
        {
            try
            {
                using (var db = new HexWriterContext())
                {
                    if (!db.Users.Any(u => u.Role == "Admin"))
                    {
                        db.Users.Add(new User
                        {
                            Username     = "admin",
                            Email        = "admin@hexwriter.com",
                            DisplayName  = "Administrator",
                            PasswordHash = BCrypt.Net.BCrypt.HashPassword("ChangeMe123!"),
                            Role         = "Admin",
                            IsActive     = true,
                            CreatedAt    = DateTime.UtcNow
                        });
                        db.SaveChanges();
                    }
                }
            }
            catch
            {
                // Users table may not exist yet — run Update-Database first
            }
        }

        protected void Application_AcquireRequestState(object sender, EventArgs e)
        {
            var supported = new[] { "en", "es" };
            string lang = "en";
            var cookie = Request.Cookies["hw_lang"];
            if (cookie != null && Array.Exists(supported, l => l == cookie.Value))
                lang = cookie.Value;
            var culture = new CultureInfo(lang);
            Thread.CurrentThread.CurrentCulture   = culture;
            Thread.CurrentThread.CurrentUICulture = culture;
        }

        protected void Application_Error(object sender, EventArgs e)
        {
            Exception exception = Server.GetLastError();

            string logPath = Server.MapPath("~/App_Data/Logs/");
            if (!Directory.Exists(logPath))
                Directory.CreateDirectory(logPath);

            string logFile = Path.Combine(logPath, string.Format("errors_{0:yyyyMMdd}.log", DateTime.Now));
            File.AppendAllText(logFile, string.Format("[{0:yyyy-MM-dd HH:mm:ss}] {1}\n\n", DateTime.Now, exception));
        }
    }
}
