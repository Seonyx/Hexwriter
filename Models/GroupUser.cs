using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace HexWriter.Web.Models
{
    [Table("GroupUsers")]
    public class GroupUser
    {
        public int Id { get; set; }
        public int GroupId { get; set; }
        public int UserId { get; set; }
        public DateTime AddedAt { get; set; }

        public Group Group { get; set; }
        public User User { get; set; }
    }
}
