using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using worbench.Data;
using worbench.Models;

namespace worbench.Services
{
    public class MechanicService : IMechanicService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly WorkshopDbContext _context;

        public MechanicService(UserManager<ApplicationUser> userManager, WorkshopDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        // Pobranie przypisanych zleceń dla mechanika
        public async Task<IEnumerable<ServiceOrder>> GetAssignedOrders(ClaimsPrincipal user)
        {
            var appUser = await _userManager.GetUserAsync(user);
            if (appUser == null) return new List<ServiceOrder>();

            return await _context.ServiceOrders
                .Include(o => o.Vehicle)
                .Include(o => o.ServiceTasks)
                    .ThenInclude(t => t.UsedParts)
                        .ThenInclude(p => p.Part)
                .Where(o => o.AssignedMechanicId == appUser.Id)
                .ToListAsync();
        }

        // Pobranie zlecenia po ID
        public async Task<ServiceOrder> GetOrderById(int id, ClaimsPrincipal user)
        {
            var appUser = await _userManager.GetUserAsync(user);
            if (appUser == null) return null;

            return await _context.ServiceOrders
                .Include(o => o.Vehicle)
                .Include(o => o.ServiceTasks)
                .FirstOrDefaultAsync(o => o.Id == id && o.AssignedMechanicId == appUser.Id);
        }

        // Pobranie zadania serwisowego po ID
        public async Task<ServiceTask> GetServiceTaskById(int taskId, ClaimsPrincipal user)
        {
            var appUser = await _userManager.GetUserAsync(user);
            if (appUser == null) return null;

            var task = await _context.ServiceTasks
                .Include(t => t.ServiceOrder)
                    .ThenInclude(o => o.Vehicle)
                .FirstOrDefaultAsync(t => t.Id == taskId && t.ServiceOrder.AssignedMechanicId == appUser.Id);

            return task;
        }

        // Aktualizacja statusu zlecenia
        public async Task<(IActionResult Result, string Message)> UpdateOrderStatus(int orderId, string status, ClaimsPrincipal user)
        {
            var order = await GetOrderById(orderId, user);
            if (order == null)
            {
                return (new NotFoundResult(), "Zlecenie nie zostało znalezione.");
            }

            order.Status = status;
            await _context.SaveChangesAsync();
            return (new RedirectToActionResult("Dashboard", "Mechanic", null), "Status zlecenia został zaktualizowany.");
        }

        // Pobranie zadań serwisowych powiązanych z danym zleceniem
        public async Task<IEnumerable<ServiceTask>> GetServiceTasks(int orderId)
        {
            return await _context.ServiceTasks
                .Include(t => t.UsedParts)
                    .ThenInclude(p => p.Part)
                .Where(t => t.ServiceOrderId == orderId)
                .ToListAsync();
        }

        // Dodanie zadania serwisowego
        public async Task<(IActionResult Result, string Message)> AddServiceTask(int orderId, string description, decimal laborCost, ClaimsPrincipal user)
        {
            var order = await GetOrderById(orderId, user);
            if (order == null)
            {
                return (new NotFoundResult(), "Zlecenie nie zostało znalezione.");
            }

            var task = new ServiceTask
            {
                ServiceOrderId = orderId,
                Description = description,
                LaborCost = laborCost
            };

            _context.ServiceTasks.Add(task);
            await _context.SaveChangesAsync();
            return (new RedirectToActionResult("ServiceTasks", "Mechanic", new { orderId }), "Czynność została dodana.");
        }

        // Pobranie użytych części dla konkretnego zadania
        public async Task<IEnumerable<UsedPart>> GetUsedParts(int taskId, ClaimsPrincipal user)
        {
            var task = await _context.ServiceTasks
                .Include(t => t.ServiceOrder)
                .FirstOrDefaultAsync(t => t.Id == taskId);

            if (task == null) return new List<UsedPart>();

            var appUser = await _userManager.GetUserAsync(user);
            if (task.ServiceOrder.AssignedMechanicId != appUser.Id)
            {
                return new List<UsedPart>();
            }

            return await _context.UsedParts
                .Include(p => p.Part)
                .Where(p => p.ServiceTaskId == taskId)
                .ToListAsync();
        }

        // Pobranie dostępnych części
        public async Task<IEnumerable<Part>> GetAvailableParts()
        {
            return await _context.Parts.ToListAsync();
        }

        // Dodanie użytej części do zadania
        public async Task<(IActionResult Result, string Message)> AddUsedPart(int taskId, int partId, int quantity, ClaimsPrincipal user)
        {
            var task = await _context.ServiceTasks
                .Include(t => t.ServiceOrder)
                .FirstOrDefaultAsync(t => t.Id == taskId);

            if (task == null)
            {
                return (new NotFoundResult(), "Czynność nie została znaleziona.");
            }

            var appUser = await _userManager.GetUserAsync(user);
            if (task.ServiceOrder.AssignedMechanicId != appUser.Id)
            {
                return (new ForbidResult(), "Nie masz uprawnień do tej czynności.");
            }

            var part = await _context.Parts.FindAsync(partId);
            if (part == null)
            {
                return (new NotFoundResult(), "Część nie została znaleziona.");
            }

            var usedPart = new UsedPart
            {
                ServiceTaskId = taskId,
                PartId = partId,
                Quantity = quantity
            };

            _context.UsedParts.Add(usedPart);
            await _context.SaveChangesAsync();
            return (new RedirectToActionResult("Parts", "Mechanic", new { taskId }), "Część została dodana.");
        }
    }
}
