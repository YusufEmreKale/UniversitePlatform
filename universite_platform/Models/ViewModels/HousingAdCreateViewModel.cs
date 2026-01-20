using System.ComponentModel.DataAnnotations;

namespace universite_platform.Models.ViewModels
{
    public class HousingAdCreateViewModel
    {
        [Display(Name = "Görsel Yükle")]
        public IFormFile? Image { get; set; }
        [Required(ErrorMessage = "Başlık zorunludur.")]
        [Display(Name = "İlan Başlığı")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Açıklama zorunludur.")]
        [Display(Name = "Açıklama")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Konum zorunludur.")]
        [Display(Name = "Konum / Adres")]
        public string Location { get; set; } = string.Empty;

        [Required(ErrorMessage = "Kira/Bütçe zorunludur.")]
        public decimal Rent { get; set; }

        [Required(ErrorMessage = "Telefon numarası zorunludur.")]
        [Phone(ErrorMessage = "Geçerli bir numara giriniz.")]
        [Display(Name = "Telefon Numarası")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "İlan türü seçimi zorunludur.")]
        [Display(Name = "İlan Türü")]
        public HousingAdType AdType { get; set; }
    }
}
