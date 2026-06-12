using System;
using System.ComponentModel.DataAnnotations;

namespace HexWriter.Web.Models.ViewModels.Admin
{
    public class BookUserAccessViewModel
    {
        public int BookUserId { get; set; }
        public string Username { get; set; }
        public string DisplayName { get; set; }
        public string AccessLevel { get; set; }
        public DateTime GrantedAt { get; set; }
    }

    public class BookGroupAccessViewModel
    {
        public int BookGroupId { get; set; }
        public string Name { get; set; }
        public string AccessLevel { get; set; }
        public DateTime GrantedAt { get; set; }
    }

    public class GrantUserAccessViewModel
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string DisplayName { get; set; }
    }

    public class GrantGroupAccessViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class UserListItemViewModel
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string DisplayName { get; set; }
        public string Role { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
    }

    public class CreateUserViewModel
    {
        [Required, MaxLength(100), Display(Name = "Username")]
        public string Username { get; set; }

        [Required, MaxLength(320), EmailAddress, Display(Name = "Email")]
        public string Email { get; set; }

        [Required, MaxLength(200), Display(Name = "Display Name")]
        public string DisplayName { get; set; }

        [Required, Display(Name = "Role")]
        public string Role { get; set; }

        [Required, DataType(DataType.Password), Display(Name = "Initial Password")]
        public string Password { get; set; }
    }

    public class EditUserViewModel
    {
        public int Id { get; set; }
        public string Username { get; set; }

        [Required, MaxLength(200), Display(Name = "Display Name")]
        public string DisplayName { get; set; }

        [Required, MaxLength(320), EmailAddress, Display(Name = "Email")]
        public string Email { get; set; }

        [Required, Display(Name = "Role")]
        public string Role { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; }
    }

    public class ResetPasswordViewModel
    {
        public int Id { get; set; }
        public string Username { get; set; }

        [Required, DataType(DataType.Password), Display(Name = "New Password")]
        public string NewPassword { get; set; }
    }
}
