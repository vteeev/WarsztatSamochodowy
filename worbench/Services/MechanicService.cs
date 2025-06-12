using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using worbench.Models;
using worbench.Data;
using worbench.DTOs;

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

        public async Task<IEnumerable<ServiceOrder>> GetAssignedOrders()
        {
            var user = await _userManager.GetUserAsync(System.Security.Claims.ClaimsPrincipal.Current);
            return await _context.ServiceOrders
                .Include(o => o.Vehicle)
                .Include(o => o.ServiceTasks)
                    .ThenInclude(t => t.UsedParts)
                        .ThenInclude(p => p.Part)
                .Where(o => o.AssignedMechanicId == user.Id)
                .ToListAsync();
        }

        public async Task<ServiceOrder> GetOrderById(int id)
        {
            var user = await _userManager.GetUserAsync(System.Security.Claims.ClaimsPrincipal.Current);
            return await _context.ServiceOrders
                .Include(o => o.Vehicle)
                .FirstOrDefaultAsync(o => o.Id == id && o.AssignedMechanicId == user.Id);
        }

        public async Task<(IActionResult Result, string Message)> UpdateOrderStatus(int orderId, string status)
        {
            var order = await GetOrderById(orderId);
            if (order == null)
            {
                return (new NotFoundResult(), "Zlecenie nie zostało znalezione.");
            }

            order.Status = status;
            await _context.SaveChangesAsync();
            return (new RedirectToActionResult("Dashboard", "Mechanic", null),
                "Status zlecenia został zaktualizowany.");
        }

        public async Task<IEnumerable<ServiceTask>> GetServiceTasks(int orderId)
        {
            var order = await GetOrderById(orderId);
            if (order == null) return new List<ServiceTask>();

            return await _context.ServiceTasks
                .Include(t => t.UsedParts)
                    .ThenInclude(p => p.Part)
                .Where(t => t.ServiceOrderId == orderId)
                .ToListAsync();
        }

        public async Task<(IActionResult Result, string Message)> AddServiceTask(int orderId, string description, decimal laborCost)
        {
            var order = await GetOrderById(orderId);
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
            return (new RedirectToActionResult("ServiceTasks", "Mechanic", new { orderId }),
                "Czynność została dodana.");
        }

        public async Task<IEnumerable<UsedPart>> GetUsedParts(int taskId)
        {
            var task = await _context.ServiceTasks
                .Include(t => t.ServiceOrder)
                .FirstOrDefaultAsync(t => t.Id == taskId);
            
            if (task == null) return new List<UsedPart>();

            var user = await _userManager.GetUserAsync(System.Security.Claims.ClaimsPrincipal.Current);
            if (task.ServiceOrder.AssignedMechanicId != user.Id)
            {
                return new List<UsedPart>();
            }

            return await _context.UsedParts
                .Include(p => p.Part)
                .Where(p => p.ServiceTaskId == taskId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Part>> GetAvailableParts()
        {
            return await _context.Parts.ToListAsync();
        }

        public async Task<(IActionResult Result, string Message)> AddUsedPart(int taskId, int partId, int quantity)
        {
            var task = await _context.ServiceTasks
                .Include(t => t.ServiceOrder)
                .FirstOrDefaultAsync(t => t.Id == taskId);
            
            if (task == null)
            {
                return (new NotFoundResult(), "Czynność nie została znaleziona.");
            }

            var user = await _userManager.GetUserAsync(System.Security.Claims.ClaimsPrincipal.Current);
            if (task.ServiceOrder.AssignedMechanicId != user.Id)
            {
                return (new NotFoundResult(), "Nie masz uprawnień do tej czynności.");
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
            return (new RedirectToActionResult("Parts", "Mechanic", new { taskId }),
                "Część została dodana.");
        }
    }
} 