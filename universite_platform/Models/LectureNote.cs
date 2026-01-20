using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace universite_platform.Models
{
    public enum ExamType
    {
        Vize,
        Final,
        Butunleme
    }

    public class LectureNote
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Başlık zorunludur.")]
        [Display(Name = "Not Başlığı")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ders adı zorunludur.")]
        [Display(Name = "Ders Adı")]
        public string CourseName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Sınav Türü")]
        public ExamType ExamType { get; set; }

        [Display(Name = "Dosya Yolu")]
        public string? FilePath { get; set; }

        [Display(Name = "Ücret (Opsiyonel)")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal? Price { get; set; }

        public bool IsPaid => Price.HasValue && Price.Value > 0;

        public DateTime UploadedAt { get; set; } = DateTime.Now;

        // Foreign Key
        public string? OwnerId { get; set; }
        [ForeignKey("OwnerId")]
        public ApplicationUser? Owner { get; set; }

        public int DownloadCount { get; set; } = 0;
        public virtual ICollection<NoteLike> Likes { get; set; } = new List<NoteLike>();
        public virtual ICollection<NoteComment> Comments { get; set; } = new List<NoteComment>();
    }
}
