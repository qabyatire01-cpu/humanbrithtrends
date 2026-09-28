using HumanBirthPredictionSystem.Data;
using HumanBirthPredictionSystem.Models;
using HumanBirthPredictionSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HumanBirthPredictionSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private readonly ApplicationDbContext _db;

        public UsersController(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index()
        {
            var users = await _db.Users
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync();
            return View(users);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new CreateUserViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateUserViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            if (await _db.Users.AnyAsync(u => u.Username.ToLower() == model.Username.ToLower()))
            {
                ModelState.AddModelError("Username", "This username is already taken.");
                return View(model);
            }

            var user = new User
            {
                Username = model.Username.Trim(),
                FullName = model.FullName.Trim(),
                Role = model.Role,
                PasswordHash = PasswordHasher.Hash(model.Password),
                CreatedAt = DateTime.UtcNow
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            TempData["Success"] = $"User '{user.Username}' ({user.Role}) created successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var user = await _db.Users.FindAsync(id);
            if (user == null) return NotFound();

            var vm = new EditUserViewModel
            {
                Id = user.Id,
                Username = user.Username,
                FullName = user.FullName,
                Role = user.Role
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EditUserViewModel model)
        {
            if (id != model.Id) return BadRequest();
            if (!ModelState.IsValid) return View(model);

            var user = await _db.Users.FindAsync(id);
            if (user == null) return NotFound();

            if (user.Username.ToLower() != model.Username.ToLower() &&
                await _db.Users.AnyAsync(u => u.Username.ToLower() == model.Username.ToLower() && u.Id != id))
            {
                ModelState.AddModelError("Username", "This username is already taken.");
                return View(model);
            }

            user.Username = model.Username.Trim();
            user.FullName = model.FullName.Trim();
            user.Role = model.Role;

            if (!string.IsNullOrWhiteSpace(model.NewPassword))
            {
                user.PasswordHash = PasswordHasher.Hash(model.NewPassword);
            }

            await _db.SaveChangesAsync();
            TempData["Success"] = $"User '{user.Username}' updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _db.Users.FindAsync(id);
            if (user == null) return NotFound();

            if (User.Identity?.Name?.ToLower() == user.Username.ToLower())
            {
                TempData["Error"] = "You cannot delete your own account while logged in.";
                return RedirectToAction(nameof(Index));
            }

            _db.Users.Remove(user);
            await _db.SaveChangesAsync();
            TempData["Success"] = $"User '{user.Username}' deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}
