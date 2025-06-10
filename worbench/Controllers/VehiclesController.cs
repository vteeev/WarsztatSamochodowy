using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using worbench.Models;
using worbench.Services;
using System.Threading.Tasks;

namespace worbench.Controllers
{
    public class VehiclesController : Controller
    {
        private readonly IVehicleService _vehicleService; // Zmieniamy wstrzyknięcie zależności na serwis
        private readonly UserManager<ApplicationUser> _userManager;

        // Konstruktor wstrzykujący zależności
        public VehiclesController(IVehicleService vehicleService, UserManager<ApplicationUser> userManager)
        {
            _vehicleService = vehicleService;
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
            var user = await _userManager.GetUserAsync(User); // Pobieranie aktualnego użytkownika
            if (user == null)
            {
                return RedirectToAction("Login", "Account"); // Jeśli użytkownik nie jest zalogowany
            }

            try
            {
                // Korzystamy z serwisu do dodania pojazdu
                await _vehicleService.AddVehicleAsync(vehicle);
                return RedirectToAction("Dashboard", "Client");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message); // Obsługuje wyjątek, np. brak klienta
                return View("Create", vehicle); // W przypadku błędu wracamy do formularza
            }
        }

        // GET: Vehicles/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var vehicle = await _vehicleService.GetVehicleByIdAsync(id); // Korzystamy z serwisu do pobrania pojazdu
            if (vehicle == null)
            {
                return NotFound(); // Jeśli pojazd nie istnieje, zwracamy NotFound
            }
            return View(vehicle); // Zwracamy widok z danymi pojazdu
        }

        // GET: Vehicles/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var vehicle = await _vehicleService.GetVehicleByIdAsync(id); // Pobieramy pojazd do edycji
            if (vehicle == null)
            {
                return NotFound();
            }
            return View(vehicle);
        }

        // POST: Vehicles/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Make,Model,Year,VIN,RegistrationNumber,ImageUrl")] Vehicle vehicle)
        {
            if (id != vehicle.Id) // Sprawdzamy, czy ID pojazdu zgadza się z ID w URL
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                await _vehicleService.UpdateVehicleAsync(vehicle); // Korzystamy z serwisu do aktualizacji pojazdu
                return RedirectToAction(nameof(Index)); // Po aktualizacji przekierowujemy na listę pojazdów
            }
            return View(vehicle);
        }

        // GET: Vehicles/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var vehicle = await _vehicleService.GetVehicleByIdAsync(id); // Pobieramy pojazd do usunięcia
            if (vehicle == null)
            {
                return NotFound();
            }
            return View(vehicle);
        }

        // POST: Vehicles/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _vehicleService.DeleteVehicleAsync(id); // Korzystamy z serwisu do usunięcia pojazdu
            return RedirectToAction(nameof(Index)); // Po usunięciu przekierowujemy na listę pojazdów
        }
    }
}
