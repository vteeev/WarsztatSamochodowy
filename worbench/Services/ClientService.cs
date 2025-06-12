using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using worbench.Models;
using worbench.Data;
using worbench.DTOs;
using worbench.Mappers;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace worbench.Services
{
    public class ClientService : IClientService
    {
        private readonly WorkshopDbContext _context;

        public ClientService(WorkshopDbContext context)
        {
            _context = context;
        }

        public async Task<(IEnumerable<Vehicle>, IEnumerable<ServiceOrderDto>)> GetDashboardData(int userId)
        {
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.UserId == userId);
            if (customer == null)
            {
                return (new List<Vehicle>(), new List<ServiceOrderDto>());
            }

            var vehicles = await _context.Vehicles.Where(v => v.CustomerId == customer.Id).ToListAsync();
            var orders = await _context.ServiceOrders
                .Include(o => o.Vehicle)
                .Include(o => o.ServiceTasks)
                    .ThenInclude(t => t.UsedParts)
                        .ThenInclude(up => up.Part)
                .Where(o => o.Vehicle.CustomerId == customer.Id)
                .ToListAsync();

            var orderDtos = orders.Select(ServiceOrderMapper.ToDtoWithCustom).ToList();
            return (vehicles, orderDtos);
        }

        public async Task<IActionResult> CreateVehicle(Vehicle vehicle, int userId, ModelStateDictionary modelState)
        {
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.UserId == userId);
            if (customer == null)
            {
                return new RedirectToActionResult("Create", "Customers", null);
            }

            vehicle.CustomerId = customer.Id;
            if (modelState.IsValid)
            {
                _context.Add(vehicle);
                await _context.SaveChangesAsync();
                return new RedirectToActionResult("Dashboard", "Client", null);
            }

            return new ViewResult 
            { 
                ViewName = "~/Views/Vehicles/Create.cshtml", 
                ViewData = new Microsoft.AspNetCore.Mvc.ViewFeatures.ViewDataDictionary<Vehicle>(new Microsoft.AspNetCore.Mvc.ModelBinding.EmptyModelMetadataProvider(), modelState)
                {
                    Model = vehicle
                }
            };
        }

        public async Task<IActionResult> CreateOrder(int vehicleId, int userId, ModelStateDictionary modelState)
        {
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.UserId == userId);
            if (customer == null)
            {
                return new RedirectToActionResult("Create", "Customers", null);
            }

            var order = new ServiceOrder
            {
                VehicleId = vehicleId,
                CreatedAt = System.DateTime.Now,
                Status = "Nowe"
            };

            if (modelState.IsValid)
            {
                _context.ServiceOrders.Add(order);
                await _context.SaveChangesAsync();
                return new RedirectToActionResult("Dashboard", "Client", null);
            }

            return new ViewResult 
            { 
                ViewName = "CreateOrder", 
                ViewData = new Microsoft.AspNetCore.Mvc.ViewFeatures.ViewDataDictionary<ServiceOrder>(new Microsoft.AspNetCore.Mvc.ModelBinding.EmptyModelMetadataProvider(), modelState)
                {
                    Model = order
                }
            };
        }

        public async Task<ServiceOrderDto> GetOrderDetails(int orderId)
        {
            var order = await _context.ServiceOrders
                .Include(o => o.Vehicle)
                .Include(o => o.ServiceTasks)
                    .ThenInclude(t => t.UsedParts)
                        .ThenInclude(up => up.Part)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            return order != null ? ServiceOrderMapper.ToDtoWithCustom(order) : null;
        }
    }
} 