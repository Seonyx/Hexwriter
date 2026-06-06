using System.Web.Mvc;
using System.Web.Routing;

namespace HexWriter.Web
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            // Auth routes
            routes.MapRoute(
                name: "AdminLogin",
                url: "admin/login",
                defaults: new { controller = "Admin", action = "Login" }
            );

            routes.MapRoute(
                name: "AdminLogout",
                url: "admin/logout",
                defaults: new { controller = "Admin", action = "Logout" }
            );

            routes.MapRoute(
                name: "AdminSettings",
                url: "admin/settings",
                defaults: new { controller = "Admin", action = "Settings" }
            );

            // Book Editor routes (must be before generic Admin route)
            routes.MapRoute(
                name: "BookEditorProjects",
                url: "admin/bookeditor/projects/{action}/{id}",
                defaults: new { controller = "BookProject", action = "Index", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "BookEditorFiles",
                url: "admin/bookeditor/files/{action}/{id}",
                defaults: new { controller = "FileUpload", action = "Index", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "BookEditorEditor",
                url: "admin/bookeditor/editor/{action}/{id}",
                defaults: new { controller = "Editor", action = "Index", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "BookEditorExport",
                url: "admin/bookeditor/export/{action}/{id}",
                defaults: new { controller = "Export", action = "Index", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "BookEditorImportLog",
                url: "admin/bookeditor/importlog/{action}/{id}",
                defaults: new { controller = "ImportLog", action = "Index", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "BookEditorDraft",
                url: "admin/bookeditor/draft/{action}/{id}",
                defaults: new { controller = "Draft", action = "Diff", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "BookEditorCharacters",
                url: "admin/bookeditor/characters/{action}/{id}",
                defaults: new { controller = "Character", action = "Index", id = UrlParameter.Optional }
            );

            // Session-free progress polling endpoint
            routes.MapRoute(
                name: "BookEditorImportProgress",
                url: "admin/bookeditor/importprogress/{action}",
                defaults: new { controller = "ImportProgress", action = "Status" }
            );

            // Generic admin catch-all
            routes.MapRoute(
                name: "Admin",
                url: "admin/{action}/{id}",
                defaults: new { controller = "Admin", action = "Dashboard", id = UrlParameter.Optional }
            );
        }
    }
}
