using System;
using System.Linq;
using System.Web.Mvc;
using HexWriter.Web.Helpers;
using HexWriter.Web.Models;
using HexWriter.Web.Models.ViewModels.Admin;

namespace HexWriter.Web.Controllers
{
    [Authorize]
    public class GroupsController : Controller
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

            var groups = db.Groups
                .OrderBy(g => g.Name)
                .ToList()
                .Select(g => new GroupListItemViewModel
                {
                    Id          = g.Id,
                    Name        = g.Name,
                    Description = g.Description,
                    MemberCount = db.GroupUsers.Count(gu => gu.GroupId == g.Id)
                })
                .ToList();

            return View(groups);
        }

        public ActionResult Create()
        {
            var check = RequireAdmin();
            if (check != null) return check;

            return View(new GroupEditViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(GroupEditViewModel model)
        {
            var check = RequireAdmin();
            if (check != null) return check;

            if (!ModelState.IsValid)
                return View(model);

            if (db.Groups.Any(g => g.Name == model.Name))
            {
                ModelState.AddModelError("Name", "A group with this name already exists.");
                return View(model);
            }

            db.Groups.Add(new Group
            {
                Name        = model.Name,
                Description = model.Description,
                CreatedAt   = DateTime.UtcNow
            });

            db.SaveChanges();
            TempData["Message"] = string.Format("Group '{0}' created.", model.Name);
            return RedirectToAction("Index");
        }

        public ActionResult Edit(int id)
        {
            var check = RequireAdmin();
            if (check != null) return check;

            var group = db.Groups.Find(id);
            if (group == null) return HttpNotFound();

            var members = db.GroupUsers
                .Where(gu => gu.GroupId == id)
                .Join(db.Users, gu => gu.UserId, u => u.Id, (gu, u) => new GroupMemberViewModel
                {
                    GroupUserId = gu.Id,
                    UserId      = u.Id,
                    Username    = u.Username,
                    DisplayName = u.DisplayName,
                    AddedAt     = gu.AddedAt
                })
                .OrderBy(m => m.Username)
                .ToList();

            var memberUserIds = members.Select(m => m.UserId).ToList();

            var allUsers = db.Users
                .Where(u => u.IsActive && !memberUserIds.Contains(u.Id))
                .OrderBy(u => u.Username)
                .Select(u => new UserSelectItem { Id = u.Id, Username = u.Username, DisplayName = u.DisplayName })
                .ToList();

            return View(new GroupEditViewModel
            {
                Id          = group.Id,
                Name        = group.Name,
                Description = group.Description,
                Members     = members,
                AllUsers    = allUsers
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(GroupEditViewModel model)
        {
            var check = RequireAdmin();
            if (check != null) return check;

            if (!ModelState.IsValid)
            {
                model.Members = db.GroupUsers.Where(gu => gu.GroupId == model.Id)
                    .Join(db.Users, gu => gu.UserId, u => u.Id, (gu, u) => new GroupMemberViewModel
                    {
                        GroupUserId = gu.Id, UserId = u.Id, Username = u.Username,
                        DisplayName = u.DisplayName, AddedAt = gu.AddedAt
                    }).OrderBy(m => m.Username).ToList();
                model.AllUsers = db.Users.Where(u => u.IsActive)
                    .Select(u => new UserSelectItem { Id = u.Id, Username = u.Username, DisplayName = u.DisplayName }).ToList();
                return View(model);
            }

            var group = db.Groups.Find(model.Id);
            if (group == null) return HttpNotFound();

            if (db.Groups.Any(g => g.Name == model.Name && g.Id != model.Id))
            {
                ModelState.AddModelError("Name", "A group with this name already exists.");
                return View(model);
            }

            group.Name        = model.Name;
            group.Description = model.Description;
            db.SaveChanges();

            TempData["Message"] = "Group updated.";
            return RedirectToAction("Edit", new { id = model.Id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddMember(int groupId, int userId)
        {
            var check = RequireAdmin();
            if (check != null) return check;

            if (!db.GroupUsers.Any(gu => gu.GroupId == groupId && gu.UserId == userId))
            {
                db.GroupUsers.Add(new GroupUser { GroupId = groupId, UserId = userId, AddedAt = DateTime.UtcNow });
                db.SaveChanges();
            }

            return RedirectToAction("Edit", new { id = groupId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RemoveMember(int groupUserId, int groupId)
        {
            var check = RequireAdmin();
            if (check != null) return check;

            var gu = db.GroupUsers.Find(groupUserId);
            if (gu != null)
            {
                db.GroupUsers.Remove(gu);
                db.SaveChanges();
            }

            return RedirectToAction("Edit", new { id = groupId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            var check = RequireAdmin();
            if (check != null) return check;

            var group = db.Groups.Find(id);
            if (group == null) return HttpNotFound();

            // Remove all memberships and book grants before deleting group
            db.GroupUsers.RemoveRange(db.GroupUsers.Where(gu => gu.GroupId == id));
            db.BookGroups.RemoveRange(db.BookGroups.Where(bg => bg.GroupId == id));
            db.Groups.Remove(group);
            db.SaveChanges();

            TempData["Message"] = string.Format("Group '{0}' deleted.", group.Name);
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
