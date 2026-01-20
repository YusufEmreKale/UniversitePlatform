using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using universite_platform.Data;
using universite_platform.Models;
using universite_platform.Models.ViewModels;

namespace universite_platform.Controllers
{
    public class ForumController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ForumController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var topics = await _context.ForumTopics
                .Include(t => t.Author)
                .Include(t => t.Comments)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
            return View(topics);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var topic = await _context.ForumTopics
                .Include(t => t.Author)
                .Include(t => t.Comments)
                    .ThenInclude(c => c.Author)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (topic == null) return NotFound();

            return View(topic);
        }

        [Authorize(Roles = "Student,Admin")]
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [Authorize(Roles = "Student,Admin")]
        [HttpPost]
        public async Task<IActionResult> Create(ForumTopicCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                 if (user == null) return RedirectToAction("Login", "Account");

                var topic = new ForumTopic
                {
                    Title = model.Title,
                    Content = model.Content,
                    AuthorId = user.Id,
                    CreatedAt = DateTime.Now
                };

                _context.Add(topic);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        [Authorize(Roles = "Student,Admin")]
        [HttpPost]
        public async Task<IActionResult> AddComment(int topicId, string content)
        {
            if (string.IsNullOrEmpty(content))
            {
                return RedirectToAction(nameof(Details), new { id = topicId });
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var comment = new ForumComment
            {
                TopicId = topicId,
                Content = content,
                AuthorId = user.Id,
                CreatedAt = DateTime.Now
            };

            _context.Add(comment);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> DeleteComment(int id)
        {
            var comment = await _context.ForumComments.FindAsync(id);
            if (comment != null)
            {
                _context.ForumComments.Remove(comment);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
