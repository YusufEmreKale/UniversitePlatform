using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using universite_platform.Data;
using universite_platform.Models;
using universite_platform.Models.ViewModels;

namespace universite_platform.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ProfileController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var viewModel = new ProfileViewModel
            {
                User = user,
                Notifications = await _context.Notifications
                    .Where(n => n.UserId == user.Id)
                    .OrderByDescending(n => n.CreatedAt)
                    .ToListAsync(),
                
                PurchasedNotes = await _context.UserPurchasedNotes
                    .Include(p => p.LectureNote)
                        .ThenInclude(n => n.Owner)
                    .Where(p => p.UserId == user.Id)
                    .OrderByDescending(p => p.PurchaseDate)
                    .ToListAsync(),

                MyNotes = await _context.LectureNotes
                    .Include(n => n.Likes) // Load likes for stats if needed
                    .Where(n => n.OwnerId == user.Id)
                    .OrderByDescending(n => n.UploadedAt)
                    .ToListAsync(),

                MyComments = await _context.NoteComments
                    .Include(c => c.LectureNote)
                    .Where(c => c.UserId == user.Id)
                    .OrderByDescending(c => c.CreatedAt)
                    .ToListAsync(),

                MyMarketItems = await _context.MarketItems
                    .Where(m => m.OwnerId == user.Id)
                    .OrderByDescending(m => m.CreatedAt)
                    .ToListAsync(),

                MyHousingAds = await _context.HousingAds
                    .Where(h => h.OwnerId == user.Id)
                    .OrderByDescending(h => h.CreatedAt)
                    .ToListAsync()
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> MarkNotificationRead(int id)
        {
            var notif = await _context.Notifications.FindAsync(id);
            if(notif != null && notif.UserId == _userManager.GetUserId(User))
            {
                notif.IsRead = true;
                await _context.SaveChangesAsync();
            }
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> UpdateInfo(string fullName, string phoneNumber)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                user.FullName = fullName;
                user.PhoneNumber = phoneNumber;
                await _userManager.UpdateAsync(user);
                TempData["SuccessMessage"] = "Profil bilgileriniz güncellendi.";
            }
            return RedirectToAction(nameof(Index), new { tab = "settings" });
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(string currentPassword, string newPassword, string confirmPassword)
        {
            if (newPassword != confirmPassword)
            {
                TempData["ErrorMessage"] = "Yeni şifreler eşleşmiyor.";
                return RedirectToAction(nameof(Index), new { tab = "settings" });
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Şifreniz başarıyla değiştirildi.";
            }
            else
            {
                TempData["ErrorMessage"] = "Şifre değiştirilemedi: " + string.Join(", ", result.Errors.Select(e => e.Description));
            }

            return RedirectToAction(nameof(Index), new { tab = "settings" });
        }
    }
}
