using System.ComponentModel.DataAnnotations;

namespace universite_platform.Models.ViewModels
{
    public class ForumTopicCreateViewModel
    {
        [Required(ErrorMessage = "Konu başlığı zorunludur.")]
        [Display(Name = "Konu Başlığı")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "İçerik zorunludur.")]
        [Display(Name = "Mesajınız")]
        public string Content { get; set; } = string.Empty;
    }
}
