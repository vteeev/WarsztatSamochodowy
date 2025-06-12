using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using worbench.Models;
using worbench.DTOs;

namespace worbench.Services
{
    public interface IClientService
    {
        Task<(IEnumerable<Vehicle>, IEnumerable<ServiceOrderDto>)> GetDashboardData(int userId);
        Task<IActionResult> CreateVehicle(Vehicle vehicle, int userId, ModelStateDictionary modelState);
        Task<IActionResult> CreateOrder(int vehicleId, int userId, ModelStateDictionary modelState);
        Task<ServiceOrderDto> GetOrderDetails(int orderId);
    }
} 