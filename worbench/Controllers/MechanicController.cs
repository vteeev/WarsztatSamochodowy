using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using worbench.Models;
using worbench.Services;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Linq;

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

        // Podstawowa walidacja istnienia zlecenia
        private bool ValidateOrderExistence(ServiceOrder order)
        {
            if (order == null)
            {
                TempData["Error"] = "Zlecenie nie zostało znalezione.";
                return false;
            }
            return true;
        }

        // Dashboard dla mechanika, wyświetla przypisane zlecenia
        public async Task<IActionResult> Dashboard()
        {
            var orders = await _mechanicService.GetAssignedOrders(User);
            
            if (orders == null || !orders.Any())
            {
                TempData["Error"] = "Brak przypisanych zleceń.";
                return View();
            }

            return View(orders);
        }

        // Wyświetla zadania serwisowe związane z danym zleceniem
        public async Task<IActionResult> ServiceTasks(int orderId)
        {
            var order = await _mechanicService.GetOrderById(orderId, User);
            if (!ValidateOrderExistence(order)) return RedirectToAction("Dashboard");

            ViewBag.Order = order;
            var tasks = await _mechanicService.GetServiceTasks(orderId);

            if (tasks == null || !tasks.Any())
            {
                TempData["Error"] = "Brak przypisanych zadań do tego zlecenia.";
                return View();
            }

            return View(tasks);
        }

        // Formularz do dodania nowego zadania do zlecenia
        [HttpGet]
        public IActionResult AddTask(int orderId)
        {
            ViewBag.OrderId = orderId;
            return View();
        }

        // Dodanie nowego zadania do zlecenia
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddTask(int orderId, string description, decimal laborCost)
        {
            if (string.IsNullOrEmpty(description) || laborCost <= 0)
            {
                TempData["Error"] = "Opis i koszt robocizny są wymagane.";
                return View();
            }

            var (result, message) = await _mechanicService.AddServiceTask(orderId, description, laborCost, User);
            TempData["Message"] = message;

            return result;
        }

        // Wyświetla użyte części w danym zadaniu
        public async Task<IActionResult> Parts(int taskId)
        {
            var task = await _mechanicService.GetServiceTaskById(taskId, User);
            if (task == null)
            {
                TempData["Error"] = "Zadanie nie zostało znalezione.";
                return RedirectToAction("ServiceTasks", new { orderId = task.ServiceOrderId });
            }

            ViewBag.Task = task;
            var parts = await _mechanicService.GetUsedParts(taskId, User);

            if (parts == null || !parts.Any())
            {
                TempData["Error"] = "Brak użytych części w tym zadaniu.";
                return View(new List<UsedPart>()); // lub odpowiedni typ modelu
            }

            return View(parts);
        }

        // Formularz do dodania nowej części do zadania
        [HttpGet]
        public async Task<IActionResult> AddPart(int taskId)
        {
            ViewBag.TaskId = taskId;
            ViewBag.Parts = await _mechanicService.GetAvailableParts();
            return View();
        }

        // Dodanie nowej części do zadania
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddPart(int taskId, int partId, int quantity)
        {
            if (quantity <= 0)
            {
                TempData["Error"] = "Ilość części musi być większa niż 0.";
                return RedirectToAction("Parts", new { taskId });
            }

            var (result, message) = await _mechanicService.AddUsedPart(taskId, partId, quantity, User);
            TempData["Message"] = message;

            return result;
        }

        // Formularz do edytowania statusu zlecenia
        [HttpGet]
        public async Task<IActionResult> EditStatus(int id)
        {
            var order = await _mechanicService.GetOrderById(id, User);
            if (!ValidateOrderExistence(order)) return RedirectToAction("Dashboard");

            ViewBag.Statuses = new[] { "Nowe", "W trakcie", "Oczekuje na części", "Zakończone", "Anulowane" };
            return View(order);
        }

        // Zmiana statusu zlecenia
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditStatus(int id, string status)
        {
            if (string.IsNullOrEmpty(status))
            {
                TempData["Error"] = "Status nie może być pusty.";
                return RedirectToAction("EditStatus", new { id });
            }

            var (result, message) = await _mechanicService.UpdateOrderStatus(id, status, User);
            TempData["Message"] = message;

            return result;
        }
    }
}
