using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace universite_platform.Models
{
    public enum HousingAdType
    {
        [Display(Name = "Kiralık Ev/Oda")]
        KiralikEv,
        [Display(Name = "Ev Arkadaşı Arıyorum")]
        EvArkadasiAriyorum,
        [Display(Name = "Kalacak Yer (Ev/Yurt) Arıyorum")]
        KalacakYerAriyorum
    }

    public class HousingAd
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "İlan başlığı zorunludur.")]
        [Display(Name = "Başlık")]
        public string Title { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Açıklama")]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Konum")]
        public string Location { get; set; } = string.Empty;

        [Display(Name = "Kira / Bütçe")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Rent { get; set; }

        [Required]
        [Display(Name = "İlan Türü")]
        public HousingAdType AdType { get; set; }

        [Display(Name = "Telefon Numarası")]
        [Required(ErrorMessage = "Telefon numarası zorunludur.")]
        [Phone(ErrorMessage = "Geçerli bir telefon numarası giriniz.")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Display(Name = "Görsel")]
        public string? ImageUrl { get; set; }

        public int ViewCount { get; set; } = 0;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Foreign Key
        public string? OwnerId { get; set; }
        [ForeignKey("OwnerId")]
        public ApplicationUser? Owner { get; set; }
    }
}
