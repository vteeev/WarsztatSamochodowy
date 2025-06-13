using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using worbench.Models;

namespace worbench.Services
{
    public interface IMechanicService
    {
        Task<IEnumerable<ServiceOrder>> GetAssignedOrders(ClaimsPrincipal user);
        Task<ServiceOrder> GetOrderById(int id, ClaimsPrincipal user);
        Task<(IActionResult Result, string Message)> UpdateOrderStatus(int orderId, string status, ClaimsPrincipal user);
        Task<IEnumerable<ServiceTask>> GetServiceTasks(int orderId);
        Task<(IActionResult Result, string Message)> AddServiceTask(int orderId, string description, decimal laborCost, ClaimsPrincipal user);
        Task<IEnumerable<UsedPart>> GetUsedParts(int taskId, ClaimsPrincipal user);
        Task<IEnumerable<Part>> GetAvailableParts();
        Task<(IActionResult Result, string Message)> AddUsedPart(int taskId, int partId, int quantity, ClaimsPrincipal user);
    }
}