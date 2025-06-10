using worbench.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace worbench.Services
{
    public interface IOrderService
    {
        Task<List<ServiceOrder>> GetAllServiceOrdersAsync();
        Task<ServiceOrder> GetServiceOrderByIdAsync(int id);
        Task AddServiceOrderAsync(ServiceOrder serviceOrder);
        Task UpdateServiceOrderAsync(ServiceOrder serviceOrder); 
        Task DeleteServiceOrderAsync(int id);  
    }

}