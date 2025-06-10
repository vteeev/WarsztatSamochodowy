using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using worbench.Models;
using worbench.Services;  // Używamy serwisów
using System.Threading.Tasks;
using System;

namespace worbench.Controllers
{
    [Authorize]
    public class ClientController : Controller
    {
        private readonly IClientService _clientService;  // Wstrzykujemy serwis
        private readonly UserManager<ApplicationUser> _userManager;

        // Konstruktor wstrzykujący zależności do kontrolera
        public ClientController(IClientService clientService, UserManager<ApplicationUser> userManager)
        {
            _clientService = clientService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Dashboard()
        {
            var user = await _userManager.GetUserAsync(User);
            var customer = await _clientService.GetCustomerByIdAsync(user.Id);  // Korzystamy z serwisu

            if (customer == null)
            {
                return RedirectToAction("Create", "Customers");
            }

            var vehicles = await _clientService.GetVehiclesByCustomerIdAsync(customer.Id);
            var orders = await _clientService.GetServiceOrdersByCustomerIdAsync(customer.Id);

            ViewBag.Vehicles = vehicles; // Przekazujemy pojazdy do widoku
            ViewBag.Orders = orders.Select(ServiceOrderMapper.ToDtoWithCustom); // Przekazujemy zlecenia do widoku

            return View();
        }

        public IActionResult CreateVehicle()
        {
            return View("~/Views/Vehicles/Create.cshtml");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateVehicle([Bind("Make,Model,Year,VIN,RegistrationNumber,ImageUrl")] Vehicle vehicle)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var customer = await _clientService.GetCustomerByIdAsync(user.Id);
            if (customer == null)
            {
                return RedirectToAction("Create", "Customers");
            }

            vehicle.CustomerId = customer.Id;
            if (ModelState.IsValid)
            {
                await _clientService.AddVehicleAsync(vehicle);
                return RedirectToAction("Dashboard", "Client");
            }
            return View("~/Views/Vehicles/Create.cshtml", vehicle);
        }

        public async Task<IActionResult> CreateOrder()
        {
            var user = await _userManager.GetUserAsync(User);
            var customer = await _clientService.GetCustomerByIdAsync(user.Id);
            if (customer == null)
            {
                return RedirectToAction("Create", "Customers");
            }

            var vehicles = await _clientService.GetVehiclesByCustomerIdAsync(customer.Id);
            ViewBag.Vehicles = vehicles;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateOrder(int VehicleId)
        {
            var user = await _userManager.GetUserAsync(User);
            var customer = await _clientService.GetCustomerByIdAsync(user.Id);
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
                await _clientService.AddServiceOrderAsync(order);
                return RedirectToAction("Dashboard", "Client");
            }

            var vehicles = await _clientService.GetVehiclesByCustomerIdAsync(customer.Id);
            ViewBag.Vehicles = vehicles;
            return View(order);
        }

        public async Task<IActionResult> Details(int id)
        {
            var order = await _clientService.GetServiceOrderByIdAsync(id);
            if (order == null) return NotFound();
            return View(order);
        }
    }
}
