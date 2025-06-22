// Models/Comment.cs
using System;
using System.ComponentModel.DataAnnotations;

namespace CarWorkshopManagementSystem.Models
{
    public class Comment
    {
        public int Id { get; set; }

        public string AuthorId { get; set; } = null!;

        // POPRAWKA: Zmieniamy właściwość na nullowalną (dodajemy '?')
        // Dzięki temu walidator nie będzie wymagał tego obiektu przy tworzeniu komentarza.
        public AppUser? Author { get; set; }

        [Required(ErrorMessage = "Treść komentarza jest wymagana.")]
        public string Content { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public int OrderId { get; set; }

        // POPRAWKA: Zmieniamy właściwość na nullowalną (dodajemy '?')
        public ServiceOrder? Order { get; set; }
    }
}