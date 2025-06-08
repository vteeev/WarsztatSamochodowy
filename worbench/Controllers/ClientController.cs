using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using worbench.Models;
using worbench.Data;
using worbench.Mappers;
using worbench.DTOs;

using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System;

namespace worbench.Controllers
{

    public class ClientController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly WorkshopDbContext _context;

        public ClientController(UserManager<ApplicationUser> userManager, WorkshopDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public async Task<IActionResult> Dashboard()
        {
            var user = await _userManager.GetUserAsync(User);
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.UserId == user.Id);

            if (customer == null)
            {
                // Przekieruj do formularza tworzenia Customer
                return RedirectToAction("Create", "Customers");
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
            return View("Dashboard", (vehicles.AsEnumerable(), orderDtos.AsEnumerable()));
        }

        // GET: Client/CreateVehicle
        public IActionResult CreateVehicle()
        {
            return View("~/Views/Vehicles/Create.cshtml");
        }

        // POST: Client/CreateVehicle
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateVehicle([Bind("Make,Model,Year,VIN,RegistrationNumber,ImageUrl")] Vehicle vehicle)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.UserId == user.Id);
            if (customer == null)
            {
                return RedirectToAction("Create", "Customers");
            }

            vehicle.CustomerId = customer.Id;
            if (ModelState.IsValid)
            {
                _context.Add(vehicle);
                await _context.SaveChangesAsync();
                return RedirectToAction("Dashboard", "Client");
            }
            // ZAWSZE jawnie podaj ścieżkę do widoku:
            foreach (var modelStateKey in ModelState.Keys)
            {
                var value = ModelState[modelStateKey];
                foreach (var error in value.Errors)
                {
                    Console.WriteLine($"Błąd w polu {modelStateKey}: {error.ErrorMessage}");
                }
            }

            return View("~/Views/Vehicles/Create.cshtml", vehicle);
        }

        // GET: Client/CreateOrder
        public async Task<IActionResult> CreateOrder()
        {
            var user = await _userManager.GetUserAsync(User);
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.UserId == user.Id);
            if (customer == null)
            {
                return RedirectToAction("Create", "Customers");
            }
            var vehicles = await _context.Vehicles.Where(v => v.CustomerId == customer.Id).ToListAsync();
            ViewBag.Vehicles = vehicles;
            return View();
        }

        // POST: Client/CreateOrder
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateOrder(int VehicleId)
        {
            var user = await _userManager.GetUserAsync(User);
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.UserId == user.Id);
            if (customer == null)
            {
                return RedirectToAction("Create", "Customers");
            }
            var order = new ServiceOrder
            {
                VehicleId = VehicleId,
                CreatedAt = DateTime.Now,
                Status = "Nowe"
            };
            if (ModelState.IsValid)
            {
                _context.ServiceOrders.Add(order);
                await _context.SaveChangesAsync();
                return RedirectToAction("Dashboard");
            }
            var vehicles = await _context.Vehicles.Where(v => v.CustomerId == customer.Id).ToListAsync();
            ViewBag.Vehicles = vehicles;
            return View(order);
        }

        public async Task<IActionResult> Details(int id)
        {
            var order = await _context.ServiceOrders
                .Include(o => o.Vehicle)
                .Include(o => o.ServiceTasks)
                    .ThenInclude(t => t.UsedParts)
                        .ThenInclude(up => up.Part)
                .FirstOrDefaultAsync(o => o.Id == id);
            if (order == null) return NotFound();
            var dto = ServiceOrderMapper.ToDtoWithCustom(order);
            return View(dto);
        }

/*
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Customer customer)
        {
            var userId = _userManager.GetUserId(User);
            customer.UserId = Guid.Parse(userId); // jeśli UserId to Guid
            _context.Add(customer);
            await _context.SaveChangesAsync();
            return RedirectToAction("Dashboard", "Client");
        }*/
    }
} 