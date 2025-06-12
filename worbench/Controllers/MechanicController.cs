using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using worbench.Models;
using worbench.Services;
using System.Threading.Tasks;
using worbench.DTOs;

namespace worbench.Controllers
{
    [Authorize(Roles = "Mechanik")]
    public class MechanicController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMechanicService _mechanicService;

        public MechanicController(UserManager<ApplicationUser> userManager, IMechanicService mechanicService)
        {
            _userManager = userManager;
            _mechanicService = mechanicService;
        }

        public async Task<IActionResult> Dashboard()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var orders = await _mechanicService.GetDashboardData(user.Id);
            return View(orders);
        }

        public async Task<IActionResult> ServiceTasks(int orderId)
        {
            var tasks = await _mechanicService.GetServiceTasks(orderId);
            return View(tasks);
        }

        [HttpGet]
        public IActionResult AddTask(int orderId)
        {
            ViewBag.OrderId = orderId;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddTask(int orderId, string description, decimal laborCost)
        {
            var result = await _mechanicService.AddServiceTask(orderId, description, laborCost);
            if (result.Message != null)
            {
                TempData["Success"] = result.Message;
            }
            return result.Result;
        }

        public async Task<IActionResult> Parts(int taskId)
        {
            var parts = await _mechanicService.GetUsedParts(taskId);
            return View(parts);
        }

        [HttpGet]
        public IActionResult AddPart(int taskId)
        {
            ViewBag.TaskId = taskId;
            ViewBag.Parts = await _mechanicService.GetAvailableParts();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddPart(int taskId, int partId, int quantity)
        {
            var result = await _mechanicService.AddUsedPart(taskId, partId, quantity);
            if (result.Message != null)
            {
                TempData["Success"] = result.Message;
            }
            return result.Result;
        }

        [HttpGet]
        public IActionResult EditStatus(int id)
        {
            var order = await _mechanicService.GetOrderById(id);
            if (order == null) return NotFound();

            ViewBag.Statuses = new[] { "Nowe", "W trakcie", "Oczekuje na części", "Zakończone", "Anulowane" };
            return View(order);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditStatus(int id, string status)
        {
            var result = await _mechanicService.UpdateOrderStatus(id, status);
            if (result.Message != null)
            {
                TempData["Success"] = result.Message;
            }
            return result.Result;
        }
    }
} 