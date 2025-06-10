using worbench.Models;
using worbench.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace worbench.Services
{
    public class ClientService : IClientService
    {
        private readonly WorkshopDbContext _context;

        public ClientService(WorkshopDbContext context)
        {
            _context = context;
        }

        public async Task<List<Vehicle>> GetVehiclesByCustomerIdAsync(int customerId)
        {
            return await _context.Vehicles.Where(v => v.CustomerId == customerId).ToListAsync();
        }

        public async Task<List<ServiceOrder>> GetServiceOrdersByCustomerIdAsync(int customerId)
        {
            return await _context.ServiceOrders
                .Include(o => o.Vehicle)
                .Include(o => o.ServiceTasks)
                .ThenInclude(t => t.UsedParts)
                .ThenInclude(up => up.Part)
                .Where(o => o.Vehicle.CustomerId == customerId)
                .ToListAsync();
        }

        public async Task AddVehicleAsync(Vehicle vehicle)
        {
            _context.Vehicles.Add(vehicle);
            await _context.SaveChangesAsync();
        }

        public async Task AddServiceOrderAsync(ServiceOrder serviceOrder)
        {
            _context.ServiceOrders.Add(serviceOrder);
            await _context.SaveChangesAsync();
        }

        public async Task<ServiceOrder> GetServiceOrderByIdAsync(int orderId)
        {
            return await _context.ServiceOrders
                .Include(o => o.Vehicle)
                .Include(o => o.ServiceTasks)
                .ThenInclude(t => t.UsedParts)
                .ThenInclude(up => up.Part)
                .FirstOrDefaultAsync(o => o.Id == orderId);
        }
    }
}