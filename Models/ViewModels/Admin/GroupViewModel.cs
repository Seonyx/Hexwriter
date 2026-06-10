using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HexWriter.Web.Models.ViewModels.Admin
{
    public class GroupListItemViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int MemberCount { get; set; }
    }

    public class GroupEditViewModel
    {
        public int Id { get; set; }

        [Required, MaxLength(200), Display(Name = "Group Name")]
        public string Name { get; set; }

        [MaxLength(500), Display(Name = "Description")]
        public string Description { get; set; }

        public List<GroupMemberViewModel> Members { get; set; } = new List<GroupMemberViewModel>();
        public List<UserSelectItem> AllUsers { get; set; } = new List<UserSelectItem>();
    }

    public class GroupMemberViewModel
    {
        public int GroupUserId { get; set; }
        public int UserId { get; set; }
        public string Username { get; set; }
        public string DisplayName { get; set; }
        public DateTime AddedAt { get; set; }
    }

    public class UserSelectItem
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string DisplayName { get; set; }
    }
}
