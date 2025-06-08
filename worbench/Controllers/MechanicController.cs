using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using worbench.Models;
using worbench.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace worbench.Controllers
{
    [Authorize(Roles = "Mechanik")]
    public class MechanicController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly WorkshopDbContext _context;

        public MechanicController(UserManager<ApplicationUser> userManager, WorkshopDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public async Task<IActionResult> Dashboard()
        {
            var user = await _userManager.GetUserAsync(User);
            var orders = _context.ServiceOrders
                .Include(o => o.Vehicle)
                .Include(o => o.ServiceTasks)
                    .ThenInclude(t => t.UsedParts)
                        .ThenInclude(up => up.Part)
                .Where(o => o.AssignedMechanicId == user.Id)
                .ToList();
            return View(orders);
        }

        public IActionResult ServiceTasks(int orderId)
        {
            var order = _context.ServiceOrders.FirstOrDefault(o => o.Id == orderId);
            if (order == null) return NotFound();
            var tasks = _context.ServiceTasks.Where(t => t.ServiceOrderId == orderId).ToList();
            ViewBag.Order = order;
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
            var task = new ServiceTask { ServiceOrderId = orderId, Description = description, LaborCost = laborCost };
            if (ModelState.IsValid)
            {
                _context.ServiceTasks.Add(task);
                await _context.SaveChangesAsync();
                return RedirectToAction("ServiceTasks", new { orderId });
            }
            ViewBag.OrderId = orderId;
            return View(task);
        }

        public IActionResult Parts(int taskId)
        {
            var task = _context.ServiceTasks.FirstOrDefault(t => t.Id == taskId);
            if (task == null) return NotFound();
            var usedParts = _context.UsedParts.Include(p => p.Part).Where(p => p.ServiceTaskId == taskId).ToList();
            ViewBag.Task = task;
            return View(usedParts);
        }

        [HttpGet]
        public IActionResult AddPart(int taskId)
        {
            var parts = _context.Parts.ToList();
            ViewBag.Parts = parts;
            ViewBag.TaskId = taskId;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddPart(int taskId, int partId, int quantity)
        {
            var usedPart = new UsedPart { ServiceTaskId = taskId, PartId = partId, Quantity = quantity };
            if (ModelState.IsValid)
            {
                _context.UsedParts.Add(usedPart);
                await _context.SaveChangesAsync();
                return RedirectToAction("Parts", new { taskId });
            }
            var parts = _context.Parts.ToList();
            ViewBag.Parts = parts;
            ViewBag.TaskId = taskId;
            return View(usedPart);
        }

        [HttpGet]
        public IActionResult EditStatus(int id)
        {
            var order = _context.ServiceOrders.FirstOrDefault(o => o.Id == id);
            if (order == null) return NotFound();
            ViewBag.Statuses = new[] { "Nowe", "W realizacji", "Gotowe" };
            return View(order);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditStatus(int id, string status)
        {
            var order = _context.ServiceOrders.FirstOrDefault(o => o.Id == id);
            if (order == null) return NotFound();
            order.Status = status;
            await _context.SaveChangesAsync();
            return RedirectToAction("Dashboard");
        }
    }
} 