using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace universite_platform.Models
{
    public class NoteComment
    {
        public int Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public int LectureNoteId { get; set; }
        [ForeignKey("LectureNoteId")]
        public virtual LectureNote LectureNote { get; set; }

        public string UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }
    }
}
