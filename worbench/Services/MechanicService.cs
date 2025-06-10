using Microsoft.EntityFrameworkCore;
using worbench.Data;
using worbench.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace worbench.Services
{
    public class MechanicService : IMechanicService
    {
        private readonly WorkshopDbContext _context;

        public MechanicService(WorkshopDbContext context)
        {
            _context = context;
        }

        // Pobierz wszystkie zlecenia przypisane do mechanika
        public async Task<List<ServiceOrder>> GetOrdersForMechanicAsync(string mechanicId)  // Use string here
        {
            return await _context.ServiceOrders
                .Include(o => o.Vehicle)
                .Include(o => o.ServiceTasks)
                    .ThenInclude(t => t.UsedParts)
                        .ThenInclude(up => up.Part)
                .Where(o => o.AssignedMechanicId.ToString() == mechanicId) // Use string comparison
                .ToListAsync();
        }

        // Pobierz zlecenie po ID
        public async Task<ServiceOrder> GetServiceOrderByIdAsync(int id)
        {
            return await _context.ServiceOrders
                .Include(o => o.Vehicle)
                .Include(o => o.ServiceTasks)
                    .ThenInclude(t => t.UsedParts)
                        .ThenInclude(up => up.Part)
                .FirstOrDefaultAsync(o => o.Id == id);
        }

        // Pobierz wszystkie zadania dla zlecenia
        public async Task<List<ServiceTask>> GetTasksForOrderAsync(int orderId)
        {
            return await _context.ServiceTasks
                .Where(t => t.ServiceOrderId == orderId)
                .ToListAsync();
        }

        // Dodaj zadanie do zlecenia
        public async Task AddTaskToOrderAsync(ServiceTask task)
        {
            _context.ServiceTasks.Add(task);
            await _context.SaveChangesAsync();
        }

        // Pobierz dostępne części
        public async Task<List<Part>> GetPartsAsync()
        {
            return await _context.Parts.ToListAsync();
        }

        // Pobierz części przypisane do zadania
        public async Task<List<UsedPart>> GetPartsForTaskAsync(int taskId)
        {
            return await _context.UsedParts
                .Include(up => up.Part)
                .Where(up => up.ServiceTaskId == taskId)
                .ToListAsync();
        }

        // Dodaj część do zadania
        public async Task AddPartToTaskAsync(UsedPart usedPart)
        {
            _context.UsedParts.Add(usedPart);
            await _context.SaveChangesAsync();
        }

        // Pobierz zadanie serwisowe po ID
        public async Task<ServiceTask> GetServiceTaskByIdAsync(int taskId)
        {
            return await _context.ServiceTasks
                .FirstOrDefaultAsync(t => t.Id == taskId);
        }

        // Aktualizuj zlecenie serwisowe
        public async Task UpdateServiceOrderAsync(ServiceOrder serviceOrder)
        {
            _context.ServiceOrders.Update(serviceOrder);
            await _context.SaveChangesAsync();
        }
    }
}
