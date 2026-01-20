using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace universite_platform.Models
{
    public class Announcement
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Başlık")]
        public string Title { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Duyuru İçeriği")]
        public string Content { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Usually Admin
        public string? AuthorId { get; set; }
        [ForeignKey("AuthorId")]
        public ApplicationUser? Author { get; set; }
    }
}
