using System.ComponentModel.DataAnnotations.Schema;

namespace universite_platform.Models
{
    public class NoteLike
    {
        public int Id { get; set; }
        
        public int LectureNoteId { get; set; }
        [ForeignKey("LectureNoteId")]
        public virtual LectureNote LectureNote { get; set; }
        
        public string UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }
    }
}
