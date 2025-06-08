using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using worbench.Models;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using worbench.Mappers;

namespace worbench.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly worbench.Data.WorkshopDbContext _context;

        public AdminController(UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager, worbench.Data.WorkshopDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }

        public IActionResult Index()
        {
            var users = _userManager.Users.ToList();
            return View(users);
        }

        [HttpPost]
        public async Task<IActionResult> ChangeRole(string userId, string newRole)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            var currentRoles = await _userManager.GetRolesAsync(user);
            var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
            if (!removeResult.Succeeded)
            {
                TempData["Error"] = "Błąd podczas usuwania starych ról: " + string.Join(", ", removeResult.Errors.Select(e => e.Description));
                return RedirectToAction("Index");
            }

            // Sprawdź, czy rola istnieje
            if (!await _roleManager.RoleExistsAsync(newRole))
            {
                TempData["Error"] = $"Rola '{newRole}' nie istnieje.";
                return RedirectToAction("Index");
            }

            var addResult = await _userManager.AddToRoleAsync(user, newRole);
            if (!addResult.Succeeded)
            {
                TempData["Error"] = "Błąd podczas dodawania roli: " + string.Join(", ", addResult.Errors.Select(e => e.Description));
                return RedirectToAction("Index");
            }

            TempData["Success"] = "Rola została zmieniona.";
            return RedirectToAction("Index");
        }

        public IActionResult Orders()
        {
            var orders = _context.ServiceOrders.Include(o => o.Vehicle).Include(o => o.AssignedMechanic).ToList();
            var orderDtos = orders.Select(Mappers.ServiceOrderMapper.ToDtoWithCustom).ToList();
            return View(orderDtos);
        }

        [HttpGet]
        public IActionResult AssignMechanic(int orderId)
        {
            var order = _context.ServiceOrders.Include(o => o.Vehicle).FirstOrDefault(o => o.Id == orderId);
            if (order == null) return NotFound();
            var mechanics = _userManager.GetUsersInRoleAsync("Mechanik").Result.ToList();
            ViewBag.Mechanics = mechanics;
            return View(order);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignMechanic(int orderId, int mechanicId)
        {
            var order = _context.ServiceOrders.FirstOrDefault(o => o.Id == orderId);
            if (order == null) return NotFound();
            order.AssignedMechanicId = mechanicId;
            await _context.SaveChangesAsync();
            TempData["Success"] = "Mechanik został przypisany do zlecenia.";
            return RedirectToAction("Orders");
        }
    }
} 