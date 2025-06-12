using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using worbench.Models;
using worbench.DTOs;

namespace worbench.Services
{
    public interface IAdminService
    {
        Task<IEnumerable<ApplicationUser>> GetAllUsers();
        Task<(IActionResult Result, string Message)> ChangeUserRole(string userId, string newRole);
        Task<IEnumerable<ServiceOrderDto>> GetAllOrders();
        Task<IActionResult> GetAssignMechanicView(int orderId);
        Task<(IActionResult Result, string Message)> AssignMechanic(int orderId, int mechanicId, ModelStateDictionary modelState);
        Task<IActionResult> GeneratePdfReport();
    }
} 