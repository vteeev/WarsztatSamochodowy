using worbench.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using worbench.Data;
using System.Collections.Generic;

namespace worbench.Services
{
    public class OrderService : IOrderService
    {
        private readonly WorkshopDbContext _context;

        public OrderService(WorkshopDbContext context)
        {
            _context = context;
        }

        // Pobieranie wszystkich zleceń
        public async Task<List<ServiceOrder>> GetAllServiceOrdersAsync()
        {
            return await _context.ServiceOrders
                .Include(o => o.Vehicle)  // Pobieramy zlecenie z pojazdem
                .Include(o => o.ServiceTasks)  // Pobieramy zlecenie z czynnościami
                    .ThenInclude(t => t.UsedParts)
                        .ThenInclude(up => up.Part) // Pobieramy części używane w czynności
                .ToListAsync();
        }

        // Pobieranie zlecenia po ID
        public async Task<ServiceOrder> GetServiceOrderByIdAsync(int id)
        {
            return await _context.ServiceOrders
                .Include(o => o.Vehicle)
                .Include(o => o.ServiceTasks)
                    .ThenInclude(t => t.UsedParts)
                        .ThenInclude(up => up.Part)
                .FirstOrDefaultAsync(o => o.Id == id);
        }

        // Dodawanie nowego zlecenia
        public async Task AddServiceOrderAsync(ServiceOrder serviceOrder)
        {
            _context.ServiceOrders.Add(serviceOrder);
            await _context.SaveChangesAsync();
        }

        // Aktualizacja zlecenia
        public async Task UpdateServiceOrderAsync(ServiceOrder serviceOrder)
        {
            _context.ServiceOrders.Update(serviceOrder);
            await _context.SaveChangesAsync();
        }

        // Usunięcie zlecenia
        public async Task DeleteServiceOrderAsync(int id)
        {
            var serviceOrder = await _context.ServiceOrders.FindAsync(id);
            if (serviceOrder != null)
            {
                _context.ServiceOrders.Remove(serviceOrder);
                await _context.SaveChangesAsync();
            }
        }
    }
}
