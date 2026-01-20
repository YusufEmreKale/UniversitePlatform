using universite_platform.Models;

namespace universite_platform.Models.ViewModels
{
    public class ProfileViewModel
    {
        public ApplicationUser User { get; set; }
        public List<Notification> Notifications { get; set; } = new List<Notification>();
        public List<UserPurchasedNote> PurchasedNotes { get; set; } = new List<UserPurchasedNote>();
        public List<LectureNote> MyNotes { get; set; } = new List<LectureNote>(); // For earnings/stats
        public List<NoteComment> MyComments { get; set; } = new List<NoteComment>();
        
        public List<MarketItem> MyMarketItems { get; set; } = new List<MarketItem>();
        public List<HousingAd> MyHousingAds { get; set; } = new List<HousingAd>();

        public decimal TotalEarnings => MyNotes.Sum(n => n.DownloadCount * (n.Price ?? 0));
        public int TotalDownloads => MyNotes.Sum(n => n.DownloadCount);
    }
}
