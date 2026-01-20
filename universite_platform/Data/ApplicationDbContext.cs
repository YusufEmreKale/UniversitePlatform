using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using universite_platform.Models;

namespace universite_platform.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<LectureNote> LectureNotes { get; set; }
        public DbSet<MarketItem> MarketItems { get; set; }
        public DbSet<HousingAd> HousingAds { get; set; }
        public DbSet<Announcement> Announcements { get; set; }
        public DbSet<ForumTopic> ForumTopics { get; set; }
        public DbSet<ForumComment> ForumComments { get; set; }
        
        public DbSet<NoteLike> NoteLikes { get; set; }
        public DbSet<NoteComment> NoteComments { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<UserPurchasedNote> UserPurchasedNotes { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            // Customize the ASP.NET Identity model and override the defaults if needed.
            // For example, you can rename the ASP.NET Identity table names and more.
            // Add your customizations after calling base.OnModelCreating(builder);
        }
    }
}
