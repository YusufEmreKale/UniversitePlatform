using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace universite_platform.Models.ViewModels
{
    public class MarketItemCreateViewModel
    {
        [Required(ErrorMessage = "İlan başlığı zorunludur.")]
        [Display(Name = "İlan Başlığı")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Açıklama zorunludur.")]
        [Display(Name = "Açıklama")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Fiyat zorunludur.")]
        [Display(Name = "Fiyat")]
        public decimal Price { get; set; }

        [Display(Name = "Kategori")]
        public string Category { get; set; } = "Genel";

        [Required(ErrorMessage = "Telefon numarası zorunludur.")]
        [Phone(ErrorMessage = "Geçerli bir numara giriniz.")]
        [Display(Name = "Telefon Numarası")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Görsel yüklemek zorunludur.")]
        [Display(Name = "Ürün Görseli")]
        public IFormFile Image { get; set; }
    }
}
