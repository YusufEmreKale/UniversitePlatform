using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using universite_platform.Data;
using universite_platform.Models;
using universite_platform.Models.ViewModels;

namespace universite_platform.Controllers
{
    public class NotesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _env;

        public NotesController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IWebHostEnvironment env)
        {
            _context = context;
            _userManager = userManager;
            _env = env;
        }

        public async Task<IActionResult> Index(string searchString, ExamType? examType)
        {
            var query = _context.LectureNotes.Include(n => n.Owner).AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(n => n.CourseName.ToLower().Contains(searchString.ToLower()) || n.Title.ToLower().Contains(searchString.ToLower()));
            }

            if (examType.HasValue)
            {
                query = query.Where(n => n.ExamType == examType.Value);
            }

            return View(await query.OrderByDescending(n => n.UploadedAt).ToListAsync());
        }

        [Authorize(Roles = "Student")]
        public async Task<IActionResult> Download(int id)
        {
            var note = await _context.LectureNotes.FindAsync(id);
            if (note == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            // Check if user is owner or note is free or note is already purchased
            bool canDownload = !note.IsPaid || 
                               note.OwnerId == user.Id || 
                               await _context.UserPurchasedNotes.AnyAsync(p => p.LectureNoteId == id && p.UserId == user.Id);

            if (!canDownload)
            {
                return RedirectToAction("Purchase", new { id = id });
            }

            note.DownloadCount++;
            _context.Update(note);
            await _context.SaveChangesAsync();

            // Notify owner if it's not their own download
            if (note.OwnerId != user.Id)
            {
                // Note: Only simplified logic here; ideally check if notification exists
            }

            string filePath = Path.Combine(_env.WebRootPath, "uploads", "notes", note.FilePath);
            if (!System.IO.File.Exists(filePath)) return NotFound();

            byte[] fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
            return File(fileBytes, "application/octet-stream", note.FilePath);
        }

        [Authorize(Roles = "Student")]
        public async Task<IActionResult> Purchase(int id)
        {
            var note = await _context.LectureNotes.FindAsync(id);
            if (note == null || !note.IsPaid) return RedirectToAction("Details", new { id });
            
            // Check if already bought
            var user = await _userManager.GetUserAsync(User);
            if (await _context.UserPurchasedNotes.AnyAsync(p => p.LectureNoteId == id && p.UserId == user.Id))
                return RedirectToAction("Download", new { id });

            return View(note);
        }

        [Authorize(Roles = "Student")]
        [HttpPost]
        public async Task<IActionResult> ProcessPayment(int id, string cardName, string cardNumber, string expiry, string cvc)
        {
            // Mock Validation
            if (string.IsNullOrWhiteSpace(cardNumber) || cardNumber.Length < 16)
            {
                TempData["Error"] = "Geçersiz kart numarası.";
                return RedirectToAction("Purchase", new { id });
            }

            var note = await _context.LectureNotes.FindAsync(id);
            if (note == null) return NotFound();
            
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var purchase = new UserPurchasedNote
            {
                LectureNoteId = id,
                UserId = user.Id,
                AmountPaid = note.Price ?? 0,
                PurchaseDate = DateTime.Now
            };

            _context.UserPurchasedNotes.Add(purchase);
            
            // Notify Owner
            var notification = new Notification
            {
                UserId = note.OwnerId, // Owner gets notified
                Title = "Tebrikler! Notunuz Satıldı 💰",
                Message = $"'{note.Title}' başlıklı notunuz {user.FullName} tarafından satın alındı. Kazancınız: {note.Price} TL.",
                CreatedAt = DateTime.Now
            };
            _context.Notifications.Add(notification);
            
            await _context.SaveChangesAsync();

            TempData["Success"] = "Ödeme başarılı! Notu şimdi indirebilirsiniz.";
            return RedirectToAction("Details", new { id });
        }


        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var note = await _context.LectureNotes
                .Include(n => n.Owner)
                .Include(n => n.Likes) // Include likes
                .Include(n => n.Comments).ThenInclude(c => c.User) // Include comments + authors
                .FirstOrDefaultAsync(m => m.Id == id);

            if (note == null) return NotFound();

            var user = await _userManager.GetUserAsync(User); // Safe to be null here? View allows anonymous? Details usually allows anonymous.
            // But if we want to check purchase, we need user.
            
            ViewBag.IsOwner = (user != null && note.OwnerId == user.Id);
            ViewBag.HasPurchased = false;

            if (user != null && note.IsPaid)
            {
                ViewBag.HasPurchased = await _context.UserPurchasedNotes
                    .AnyAsync(p => p.LectureNoteId == id && p.UserId == user.Id);
            }

            return View(note);
        }

        [Authorize(Roles = "Student")]
        public async Task<IActionResult> ViewFile(int id)
        {
            var note = await _context.LectureNotes.FindAsync(id);
            if (note == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            // Check permissions: Owner (maybe?), Purchased, or Free
            bool canView = !note.IsPaid || 
                           note.OwnerId == user.Id || 
                           await _context.UserPurchasedNotes.AnyAsync(p => p.LectureNoteId == id && p.UserId == user.Id);

            if (!canView) return RedirectToAction("Purchase", new { id }); // Should pay first

            string filePath = Path.Combine(_env.WebRootPath, "uploads", "notes", note.FilePath);
            if (!System.IO.File.Exists(filePath)) return NotFound();

            // Return inline
            var contentType = "application/pdf"; // Assuming PDF for now, or detect
            if (note.FilePath.EndsWith(".jpg") || note.FilePath.EndsWith(".png")) contentType = "image/jpeg";
            
            return File(System.IO.File.OpenRead(filePath), contentType); // No download name = inline
        }

        [Authorize(Roles = "Student,Admin")] // Admin can delete too maybe? No, specific req: Admin can verify content.
        [HttpGet]
        public IActionResult Create()
        {
             if (User.IsInRole("Admin")) return RedirectToAction("Index"); // Admin cant create
            return View();
        }

        [Authorize(Roles = "Student")]
        [HttpPost]
        public async Task<IActionResult> Create(LectureNoteCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                string uniqueFileName = null;
                if (model.File != null)
                {
                    string uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "notes");
                    if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);
                    uniqueFileName = Guid.NewGuid().ToString() + "_" + model.File.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.File.CopyToAsync(fileStream);
                    }
                }

                var user = await _userManager.GetUserAsync(User);
                 if (user == null) return RedirectToAction("Login", "Account");

                var note = new LectureNote
                {
                    Title = model.Title,
                    CourseName = model.CourseName,
                    ExamType = model.ExamType,
                    Price = model.Price,
                    FilePath = uniqueFileName,
                    OwnerId = user.Id,
                    UploadedAt = DateTime.Now
                };

                _context.Add(note);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        [Authorize(Roles = "Student")]
        [HttpPost]
        public async Task<IActionResult> AddComment(int id, string content)
        {
            var note = await _context.LectureNotes.FindAsync(id);
            if(note == null || string.IsNullOrWhiteSpace(content)) return RedirectToAction("Details", new { id });
            
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var comment = new NoteComment
            {
                LectureNoteId = id,
                UserId = user.Id,
                Content = content,
                CreatedAt = DateTime.Now
            };
            
            _context.NoteComments.Add(comment);

             if (note.OwnerId != user.Id)
            {
                 var notification = new Notification
                {
                    UserId = note.OwnerId,
                    Title = "Yeni Yorum 💬",
                    Message = $"'{note.Title}' notunuza {user.FullName} yorum yaptı: '{content}'",
                    CreatedAt = DateTime.Now
                };
                _context.Notifications.Add(notification);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Details", new { id });
        }

        [Authorize(Roles = "Student")]
        [HttpPost]
        public async Task<IActionResult> Like(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var existingLike = await _context.NoteLikes.FirstOrDefaultAsync(l => l.LectureNoteId == id && l.UserId == user.Id);

            if (existingLike == null)
            {
                var like = new NoteLike { LectureNoteId = id, UserId = user.Id };
                _context.NoteLikes.Add(like);
                
                var note = await _context.LectureNotes.FindAsync(id);
                 if (note.OwnerId != user.Id)
                {
                    var notification = new Notification
                    {
                        UserId = note.OwnerId,
                        Title = "Yeni Beğeni ❤️",
                        Message = $"'{note.Title}' notunuzu {user.FullName} beğendi.",
                        CreatedAt = DateTime.Now
                    };
                    _context.Notifications.Add(notification);
                }
            }
            else
            {
                _context.NoteLikes.Remove(existingLike);
            }
            await _context.SaveChangesAsync();
            return RedirectToAction("Details", new { id });
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Delete(int id, string reason)
        {
            var note = await _context.LectureNotes.FindAsync(id);
            if (note != null)
            {
                 // Notify Owner
                var notification = new Notification
                {
                    UserId = note.OwnerId,
                    Title = "Notunuz Silindi ⚠️",
                    Message = $"'{note.Title}' başlıklı notunuz yönetici tarafından şu gerekçeyle silindi: {reason}",
                    CreatedAt = DateTime.Now
                };
                _context.Notifications.Add(notification);

                 // Delete file too
                 string filePath = Path.Combine(_env.WebRootPath, "uploads", "notes", note.FilePath);
                 if (System.IO.File.Exists(filePath)) System.IO.File.Delete(filePath);

                _context.LectureNotes.Remove(note);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
