using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using worbench.Models;
using worbench.Services;
using System.Security.Claims;
using System.Threading.Tasks;

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
            var orders = await _mechanicService.GetAssignedOrders(User);
            return View(orders);
        }

        public async Task<IActionResult> ServiceTasks(int orderId)
        {
            var order = await _mechanicService.GetOrderById(orderId, User);
            if (order == null)
            {
                return NotFound();
            }

            ViewBag.Order = order;

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
            var (result, message) = await _mechanicService.AddServiceTask(orderId, description, laborCost, User);
            if (!string.IsNullOrEmpty(message))
            {
                TempData["Success"] = message;
            }
            return result;
        }

        public async Task<IActionResult> Parts(int taskId)
        {
            var parts = await _mechanicService.GetUsedParts(taskId, User);
            return View(parts);
        }

        [HttpGet]
        public async Task<IActionResult> AddPart(int taskId)
        {
            ViewBag.TaskId = taskId;
            ViewBag.Parts = await _mechanicService.GetAvailableParts();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddPart(int taskId, int partId, int quantity)
        {
            var (result, message) = await _mechanicService.AddUsedPart(taskId, partId, quantity, User);
            if (!string.IsNullOrEmpty(message))
            {
                TempData["Success"] = message;
            }
            return result;
        }

        [HttpGet]
        public async Task<IActionResult> EditStatus(int id)
        {
            var order = await _mechanicService.GetOrderById(id, User);
            if (order == null) return NotFound();

            ViewBag.Statuses = new[] { "Nowe", "W trakcie", "Oczekuje na części", "Zakończone", "Anulowane" };
            return View(order);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditStatus(int id, string status)
        {
            var (result, message) = await _mechanicService.UpdateOrderStatus(id, status, User);
            if (!string.IsNullOrEmpty(message))
            {
                TempData["Success"] = message;
            }
            return result;
        }
    }
}
