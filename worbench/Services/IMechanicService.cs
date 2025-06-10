using worbench.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace worbench.Services
{
    public interface IMechanicService
    {
        Task<List<ServiceOrder>> GetOrdersForMechanicAsync(string mechanicId);
        Task<ServiceOrder> GetServiceOrderByIdAsync(int id);
        Task<List<ServiceTask>> GetTasksForOrderAsync(int orderId);
        Task AddTaskToOrderAsync(ServiceTask task);
        Task<List<Part>> GetPartsAsync();
        Task<List<UsedPart>> GetPartsForTaskAsync(int taskId);
        Task AddPartToTaskAsync(UsedPart usedPart); // Dodajemy metodę do interfejsu
        Task<ServiceTask> GetServiceTaskByIdAsync(int taskId);
        Task UpdateServiceOrderAsync(ServiceOrder serviceOrder);
    }

}