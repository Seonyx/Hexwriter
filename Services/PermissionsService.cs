using System.Linq;
using HexWriter.Web.Models;

namespace HexWriter.Web.Services
{
    public class PermissionsService
    {
        private readonly HexWriterContext _db;

        public PermissionsService(HexWriterContext db)
        {
            _db = db;
        }

        // Returns "Edit", "Read", or null (no access).
        // Caller should treat Admin users as always having "Edit" — check before calling.
        public string GetEffectiveAccess(int userId, int bookProjectId)
        {
            // Direct user grant
            var direct = _db.BookUsers
                .Where(bu => bu.UserId == userId && bu.BookProjectID == bookProjectId)
                .Select(bu => bu.AccessLevel)
                .FirstOrDefault();

            // Group grants: find all groups this user belongs to, then check BookGroups
            var groupAccesses = _db.BookGroups
                .Where(bg => bg.BookProjectID == bookProjectId &&
                             _db.GroupUsers.Any(gu => gu.GroupId == bg.GroupId && gu.UserId == userId))
                .Select(bg => bg.AccessLevel)
                .ToList();

            if (direct != null)
                groupAccesses.Add(direct);

            if (!groupAccesses.Any())
                return null;

            // Edit beats Read
            return groupAccesses.Any(a => a == "Edit") ? "Edit" : "Read";
        }

        public bool CanAccess(int userId, int bookProjectId)
        {
            return GetEffectiveAccess(userId, bookProjectId) != null;
        }

        public bool CanEdit(int userId, int bookProjectId)
        {
            return GetEffectiveAccess(userId, bookProjectId) == "Edit";
        }
    }
}
