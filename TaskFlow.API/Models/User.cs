using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema; // ForeignKey etiketini kullanabilmek için ekledik

namespace TaskFlow.API.Models
{
    public class User
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string Role { get; set; }

        // EF Core'a görev bağlantısını DeveloperId üzerinden yapmasını söylüyoruz
        [ForeignKey("DeveloperId")]
        public ICollection<TaskItem> AssignedTasks { get; set; } = new List<TaskItem>();

        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    }
}