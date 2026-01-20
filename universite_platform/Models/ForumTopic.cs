using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace universite_platform.Models
{
    public class ForumTopic
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Konu Başlığı")]
        public string Title { get; set; } = string.Empty;

        [Required]
        [Display(Name = "İçerik")]
        public string Content { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public string? AuthorId { get; set; }
        [ForeignKey("AuthorId")]
        public ApplicationUser? Author { get; set; }

        public ICollection<ForumComment> Comments { get; set; } = new List<ForumComment>();
    }

    public class ForumComment
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Yorum")]
        public string Content { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public int TopicId { get; set; }
        [ForeignKey("TopicId")]
        public ForumTopic? Topic { get; set; }

        public string? AuthorId { get; set; }
        [ForeignKey("AuthorId")]
        public ApplicationUser? Author { get; set; }
    }
}
