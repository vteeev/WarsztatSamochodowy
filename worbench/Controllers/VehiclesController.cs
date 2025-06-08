using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using worbench.Models;
using worbench.Data;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace worbench.Controllers
{

    public class VehiclesController : Controller
    {
        private readonly WorkshopDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public VehiclesController(WorkshopDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Vehicles/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Vehicles/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateVehicle([Bind("Make,Model,Year,VIN,RegistrationNumber,ImageUrl")] Vehicle vehicle)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var customerr = await _context.Customers.FirstOrDefaultAsync(c => c.UserId == user.Id);
            if (customerr == null)
            {
                // Jeśli nie ma klienta, przekieruj do utworzenia profilu klienta
                return RedirectToAction("Create", "Customers");
            }
            vehicle.CustomerId = customerr.Id;
            if (ModelState.IsValid)
            {
                _context.Add(vehicle);
                await _context.SaveChangesAsync();
                return RedirectToAction("Dashboard", "Client");
            }
            return View("Vehicles/Create", vehicle);
        }
    }
} 
