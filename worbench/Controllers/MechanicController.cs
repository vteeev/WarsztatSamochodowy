using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using worbench.Models;
using worbench.Services;  // Używamy serwisów
using System.Threading.Tasks;

namespace worbench.Controllers
{
    [Authorize(Roles = "Mechanik")]
    public class MechanicController : Controller
    {
        private readonly IMechanicService _mechanicService;
        private readonly UserManager<ApplicationUser> _userManager;

        // Konstruktor wstrzykujący zależności do kontrolera
        public MechanicController(IMechanicService mechanicService, UserManager<ApplicationUser> userManager)
        {
            _mechanicService = mechanicService;
            _userManager = userManager;
        }

        // Wyświetlenie dashboardu mechanika
        public async Task<IActionResult> Dashboard()
        {
            var user = await _userManager.GetUserAsync(User);
            var orders = await _mechanicService.GetOrdersForMechanicAsync(user.Id.ToString());  // Pass mechanicId as string

            return View(orders);
        }

        // Wyświetlenie zadań dla konkretnego zlecenia
        public async Task<IActionResult> ServiceTasks(int orderId)
        {
            var tasks = await _mechanicService.GetTasksForOrderAsync(orderId);  // Pobieramy zadania zlecenia
            var order = await _mechanicService.GetServiceOrderByIdAsync(orderId);  // Pobieramy zlecenie

            if (order == null)
            {
                return NotFound();  // Jeśli zlecenie nie istnieje, zwracamy 404
            }

            ViewBag.Order = order;
            return View(tasks);  // Zwracamy widok z zadaniami
        }

        // Dodanie nowego zadania do zlecenia
        [HttpGet]
        public IActionResult AddTask(int orderId)
        {
            ViewBag.OrderId = orderId;
            return View();  // Zwracamy widok formularza
        }

        // Dodanie nowego zadania (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddTask(int orderId, string description, decimal laborCost)
        {
            var task = new ServiceTask { ServiceOrderId = orderId, Description = description, LaborCost = laborCost };

            if (ModelState.IsValid)
            {
                await _mechanicService.AddTaskToOrderAsync(task);  // Korzystamy z serwisu do dodania zadania
                return RedirectToAction("ServiceTasks", new { orderId });
            }

            ViewBag.OrderId = orderId;
            return View(task);  // Jeśli dane są niepoprawne, wracamy do formularza
        }

        // Wyświetlenie części użytych w zadaniu
        public async Task<IActionResult> Parts(int taskId)
        {
            var usedParts = await _mechanicService.GetPartsForTaskAsync(taskId);  // Pobieramy części z serwisu
            var task = await _mechanicService.GetServiceTaskByIdAsync(taskId);  // Pobieramy zadanie

            if (task == null)
            {
                return NotFound();  // Jeśli zadanie nie istnieje, zwracamy 404
            }

            ViewBag.Task = task;
            return View(usedParts);  // Zwracamy widok z częściami
        }

        // Dodanie nowej części do zadania
        [HttpGet]
        public async Task<IActionResult> AddPart(int taskId)
        {
            var parts = await _mechanicService.GetPartsAsync();  // Pobieramy części z serwisu
            ViewBag.Parts = parts;
            ViewBag.TaskId = taskId;
            return View();  // Zwracamy formularz
        }

        // Dodanie nowej części (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddPart(int taskId, int partId, int quantity)
        {
            var usedPart = new UsedPart { ServiceTaskId = taskId, PartId = partId, Quantity = quantity };

            if (ModelState.IsValid)
            {
                await _mechanicService.AddPartToTaskAsync(usedPart);  // Korzystamy z serwisu do dodania części
                return RedirectToAction("Parts", new { taskId });
            }

            var parts = await _mechanicService.GetPartsAsync();  // Pobieramy części w razie błędu
            ViewBag.Parts = parts;
            ViewBag.TaskId = taskId;
            return View(usedPart);  // Zwracamy formularz z błędami
        }

        // Zmiana statusu zlecenia
        [HttpGet]
        public async Task<IActionResult> EditStatus(int id)
        {
            var order = await _mechanicService.GetServiceOrderByIdAsync(id);  // Pobieramy zlecenie

            if (order == null)
            {
                return NotFound();  // Jeśli zlecenie nie istnieje, zwracamy 404
            }

            ViewBag.Statuses = new[] { "Nowe", "W realizacji", "Gotowe" };  // Dostępne statusy
            return View(order);  // Zwracamy widok z formularzem
        }

        // Zmiana statusu (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditStatus(int id, string status)
        {
            var order = await _mechanicService.GetServiceOrderByIdAsync(id);  // Pobieramy zlecenie

            if (order == null)
            {
                return NotFound();  // Jeśli zlecenie nie istnieje, zwracamy 404
            }

            order.Status = status;
            await _mechanicService.UpdateServiceOrderAsync(order);  // Korzystamy z serwisu do zapisania zmiany statusu
            return RedirectToAction("Dashboard");  // Po zapisaniu przekierowujemy na dashboard
        }
    }
}
