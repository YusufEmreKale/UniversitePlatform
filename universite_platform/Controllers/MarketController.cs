using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using universite_platform.Data;
using universite_platform.Models;
using universite_platform.Models.ViewModels;

namespace universite_platform.Controllers
{
    public class MarketController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _env;

        public MarketController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IWebHostEnvironment env)
        {
            _context = context;
            _userManager = userManager;
            _env = env;
        }

        public async Task<IActionResult> Index(string searchString, string category)
        {
            var query = _context.MarketItems.Include(m => m.Owner).AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(m => m.Title.ToLower().Contains(searchString.ToLower()) || m.Description.ToLower().Contains(searchString.ToLower()));
            }

            if (!string.IsNullOrEmpty(category))
            {
                query = query.Where(m => m.Category == category);
            }

            return View(await query.OrderByDescending(m => m.CreatedAt).ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var marketItem = await _context.MarketItems
                .Include(m => m.Owner)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (marketItem == null) return NotFound();

            marketItem.ViewCount++;
            _context.Update(marketItem);
            await _context.SaveChangesAsync();

            return View(marketItem);
        }

        [Authorize(Roles = "Student")]
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [Authorize(Roles = "Student")]
        [HttpPost]
        public async Task<IActionResult> Create(MarketItemCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                string uniqueFileName = null;
                if (model.Image != null)
                {
                    string uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "market");
                    if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);
                    uniqueFileName = Guid.NewGuid().ToString() + "_" + model.Image.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.Image.CopyToAsync(fileStream);
                    }
                }

                var user = await _userManager.GetUserAsync(User);
                 if (user == null) return RedirectToAction("Login", "Account");

                var item = new MarketItem
                {
                    Title = model.Title,
                    Description = model.Description,
                    Price = model.Price,
                    Category = model.Category,
                    PhoneNumber = model.PhoneNumber,
                    ImageUrl = uniqueFileName,
                    OwnerId = user.Id,
                    CreatedAt = DateTime.Now
                };

                _context.Add(item);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Delete(int id, string reason)
        {
            var item = await _context.MarketItems.FindAsync(id);
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

                _context.MarketItems.Remove(item);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
