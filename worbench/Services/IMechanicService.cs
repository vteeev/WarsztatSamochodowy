using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using worbench.Models;
using worbench.DTOs;

namespace worbench.Services
{
    public interface IMechanicService
    {
        Task<IEnumerable<ServiceOrder>> GetDashboardData(int userId);
        Task<IActionResult> GetServiceTasks(int orderId);
        Task<IActionResult> AddTask(int orderId, string description, decimal laborCost, ModelStateDictionary modelState);
        Task<IActionResult> GetParts(int taskId);
        Task<IActionResult> AddPart(int taskId, int partId, int quantity, ModelStateDictionary modelState);
        Task<IActionResult> EditStatus(int id, string status, ModelStateDictionary modelState);
        Task<IEnumerable<ServiceOrder>> GetAssignedOrders();
        Task<ServiceOrder> GetOrderById(int id);
        Task<(IActionResult Result, string Message)> UpdateOrderStatus(int orderId, string status);
        Task<IEnumerable<ServiceTask>> GetServiceTasks(int orderId);
        Task<(IActionResult Result, string Message)> AddServiceTask(int orderId, string description, decimal laborCost);
        Task<IEnumerable<UsedPart>> GetUsedParts(int taskId);
        Task<IEnumerable<Part>> GetAvailableParts();
        Task<(IActionResult Result, string Message)> AddUsedPart(int taskId, int partId, int quantity);
    }
} 