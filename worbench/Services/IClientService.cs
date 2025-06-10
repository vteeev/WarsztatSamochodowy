using worbench.Models;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace worbench.Services
{
    public interface IClientService
    {
        Task<List<Vehicle>> GetVehiclesByCustomerIdAsync(int customerId);
        Task<List<ServiceOrder>> GetServiceOrdersByCustomerIdAsync(int customerId);
        Task AddVehicleAsync(Vehicle vehicle);
        Task AddServiceOrderAsync(ServiceOrder serviceOrder);
        Task<ServiceOrder> GetServiceOrderByIdAsync(int orderId);
    }
}