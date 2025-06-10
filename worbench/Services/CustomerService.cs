using worbench.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using worbench.Data;
using System.Collections.Generic;

namespace worbench.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly WorkshopDbContext _context;

        public CustomerService(WorkshopDbContext context)
        {
            _context = context;
        }

        public async Task<Customer> GetCustomerByIdAsync(int id)
        {
            return await _context.Customers.FirstOrDefaultAsync(c => c.Id == id);
        }
        public async Task<List<Customer>> GetAllCustomersAsync()
        {
            return await _context.Customers.ToListAsync(); // Pobieramy wszystkich klientów
        }

        public async Task AddCustomerAsync(Customer customer)
        {
            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Vehicle>> GetVehiclesByCustomerIdAsync(int customerId)
        {
            return await _context.Vehicles.Where(v => v.CustomerId == customerId).ToListAsync();
        }
    }
}