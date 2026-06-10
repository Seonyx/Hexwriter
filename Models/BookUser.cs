using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HexWriter.Web.Models
{
    [Table("BookUsers")]
    public class BookUser
    {
        public int Id { get; set; }
        public int BookProjectID { get; set; }
        public int UserId { get; set; }

        [Required, MaxLength(20)]
        public string AccessLevel { get; set; }

        public DateTime GrantedAt { get; set; }

        public BookProject BookProject { get; set; }
        public User User { get; set; }
    }
}
