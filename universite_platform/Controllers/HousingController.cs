using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using universite_platform.Data;
using universite_platform.Models;
using universite_platform.Models.ViewModels;

namespace universite_platform.Controllers
{
    public class HousingController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public HousingController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(string searchString, HousingAdType? adType)
        {
            var query = _context.HousingAds.Include(h => h.Owner).AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(h => h.Location.ToLower().Contains(searchString.ToLower()) || h.Title.ToLower().Contains(searchString.ToLower()));
            }

            if (adType.HasValue)
            {
                query = query.Where(h => h.AdType == adType.Value);
            }

            return View(await query.OrderByDescending(h => h.CreatedAt).ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var housingAd = await _context.HousingAds
                .Include(h => h.Owner)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (housingAd == null) return NotFound();

            housingAd.ViewCount++;
            _context.Update(housingAd);
            await _context.SaveChangesAsync();

            return View(housingAd);
        }

        [Authorize(Roles = "Student")]
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [Authorize(Roles = "Student")]
        [HttpPost]
        public async Task<IActionResult> Create(HousingAdCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                 if (user == null) return RedirectToAction("Login", "Account");

                // Handle Image Upload
                string? imagePath = null;
                if (model.Image != null)
                {
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "housing");
                    if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);
                    
                    var uniqueFileName = Guid.NewGuid().ToString() + "_" + model.Image.FileName;
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.Image.CopyToAsync(fileStream);
                    }
                    imagePath = "/uploads/housing/" + uniqueFileName;
                }

                var ad = new HousingAd
                {
                    Title = model.Title,
                    Description = model.Description,
                    Location = model.Location,
                    Rent = model.Rent,
                    AdType = model.AdType,
                    PhoneNumber = model.PhoneNumber,
                    OwnerId = user.Id,
                    ImageUrl = imagePath,
                    CreatedAt = DateTime.Now
                };

                _context.Add(ad);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Delete(int id, string reason)
        {
            var item = await _context.HousingAds.FindAsync(id);
            if (item != null)
            {
                 // Notify Owner
                var notification = new Notification
                {
                    UserId = item.OwnerId,
                    Title = "İlanınız Silindi ⚠️",
                    Message = $"'{item.Title}' başlıklı ilanınız yönetici tarafından şu gerekçeyle silindi: {reason}",
                    CreatedAt = DateTime.Now
                };
                _context.Notifications.Add(notification);

                _context.HousingAds.Remove(item);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
