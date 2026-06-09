using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HexWriter.Web.Models
{
    [Table("Users")]
    public class User
    {
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Username { get; set; }

        [Required, MaxLength(320)]
        public string Email { get; set; }

        [Required, MaxLength(200)]
        public string DisplayName { get; set; }

        [Required, MaxLength(255)]
        public string PasswordHash { get; set; }

        [Required, MaxLength(50)]
        public string Role { get; set; }

        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
    }
}
