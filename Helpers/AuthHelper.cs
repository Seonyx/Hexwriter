using System.Web;
using System.Web.Security;

namespace HexWriter.Web.Helpers
{
    public class CurrentUser
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Role { get; set; }

        public bool IsAdmin { get { return Role == "Admin"; } }
    }

    public static class AuthHelper
    {
        public static CurrentUser GetCurrentUser(HttpContextBase httpContext)
        {
            if (!httpContext.User.Identity.IsAuthenticated)
                return null;

            var identity = httpContext.User.Identity as FormsIdentity;
            if (identity == null)
                return null;

            var parts = identity.Ticket.UserData.Split('|');
            if (parts.Length != 3)
                return null;

            int id;
            if (!int.TryParse(parts[0], out id))
                return null;

            return new CurrentUser
            {
                Id = id,
                Username = parts[1],
                Role = parts[2]
            };
        }
    }
}
