using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using worbench.Models;
using worbench.Data;
using worbench.Mappers;
using worbench.DTOs;
using worbench.Services;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace worbench.Controllers
{
    [Authorize]
    public class ClientController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IClientService _clientService;
        private readonly WorkshopDbContext _context;

        public ClientController(UserManager<ApplicationUser> userManager, IClientService clientService, WorkshopDbContext context)
        {
            _userManager = userManager;
            _clientService = clientService;
            _context = context;
        }

        public async Task<IActionResult> Dashboard()
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

            var (vehicles, orders) = await _clientService.GetDashboardData(user.Id);
            return View("Dashboard", (vehicles, orders));
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

            return await _clientService.CreateVehicle(vehicle, user.Id, ModelState);
        }

        // GET: Client/CreateOrder
        public async Task<IActionResult> CreateOrder()
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
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            return await _clientService.CreateOrder(VehicleId, user.Id, ModelState);
        }

        public async Task<IActionResult> Details(int id)
        {
            var orderDto = await _clientService.GetOrderDetails(id);
            if (orderDto == null) return NotFound();
            return View(orderDto);
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