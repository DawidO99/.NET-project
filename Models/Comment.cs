// Models/Comment.cs
using System;
using System.ComponentModel.DataAnnotations;

namespace CarWorkshopManagementSystem.Models
{
    public class Comment
    {
        public int Id { get; set; }

        public string AuthorId { get; set; } = null!;
        public AppUser Author { get; set; } = null!;

        [Required]
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public int OrderId { get; set; }
        public ServiceOrder Order { get; set; } = null!;

        //public DateTime CommentDate { get; set; }
    }
}