using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace universite_platform.Models
{
    public class UserPurchasedNote
    {
        public int Id { get; set; }
        
        public int LectureNoteId { get; set; }
        [ForeignKey("LectureNoteId")]
        public virtual LectureNote LectureNote { get; set; }

        public string UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }

        public DateTime PurchaseDate { get; set; } = DateTime.Now;
        public decimal AmountPaid { get; set; } = 0;
    }
}
