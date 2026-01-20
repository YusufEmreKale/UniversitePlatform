using System.ComponentModel.DataAnnotations;
using universite_platform.Models;

namespace universite_platform.Models.ViewModels
{
    public class LectureNoteCreateViewModel
    {
        [Required(ErrorMessage = "Başlık zorunludur.")]
        [Display(Name = "Not Başlığı")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ders adı zorunludur.")]
        [Display(Name = "Ders Adı")]
        public string CourseName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Sınav Türü")]
        public ExamType ExamType { get; set; }

        [Required(ErrorMessage = "Lütfen bir dosya yükleyin.")]
        [Display(Name = "Ders Notu Dosyası (PDF, Word, vs.)")]
        public IFormFile File { get; set; } = null!;

        [Display(Name = "Ücret (Opsiyonel - Boş bırakılırsa ücretsiz)")]
        public decimal? Price { get; set; }
    }
}
