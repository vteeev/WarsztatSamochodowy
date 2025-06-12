using Microsoft.AspNetCore.Authorization;
using PdfSharpCore.Pdf;
using PdfSharpCore.Drawing;
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
using Microsoft.Extensions.Logging;  // Dodaj przestrzeń nazw dla ILogger

namespace worbench.Controllers
{
    public class ClientController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly WorkshopDbContext _context;
        private readonly ILogger<ClientController> _logger;  // Deklaracja loggera

        // Wstrzyknięcie zależności do loggera
        public ClientController(UserManager<ApplicationUser> userManager, WorkshopDbContext context, ILogger<ClientController> logger)
        {
            _userManager = userManager;
            _context = context;
            _logger = logger;  // Inicjalizacja loggera
        }
 // Generowanie raportu PDF
    public async Task<IActionResult> GenerateReport()
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

        var orders = await _context.ServiceOrders
            .Include(o => o.Vehicle)
            .Where(o => o.Vehicle.CustomerId == customer.Id)
            .ToListAsync();

        // Tworzenie dokumentu PDF
        var document = new PdfDocument();
        document.Info.Title = "Raport zleceń serwisowych";

        // Dodanie strony
        var page = document.AddPage();
        var gfx = XGraphics.FromPdfPage(page);
        var font = new XFont("Verdana", 12, XFontStyle.Regular);

        // Dodanie tytułu raportu
        int yPoint = 20;
        gfx.DrawString("Raport zleceń serwisowych dla klienta", font, XBrushes.Black, new XPoint(20, yPoint));
        yPoint += 20;

        // Iterowanie przez zlecenia klienta
        foreach (var order in orders)
        {
            gfx.DrawString($"Zlecenie ID: {order.Id}", font, XBrushes.Black, new XPoint(20, yPoint));
            yPoint += 20;
            gfx.DrawString($"Pojazd: {order.Vehicle?.Make} {order.Vehicle?.Model}", font, XBrushes.Black, new XPoint(20, yPoint));
            yPoint += 20;
            gfx.DrawString($"Status: {order.Status}", font, XBrushes.Black, new XPoint(20, yPoint));
            yPoint += 20;

            // Iterowanie przez zadania w zleceniu
            foreach (var task in order.ServiceTasks)
            {
                gfx.DrawString($"Czynność: {task.Description} - Koszt robocizny: {task.LaborCost} PLN", font, XBrushes.Black, new XPoint(20, yPoint));
                yPoint += 20;

                // Iterowanie przez części użyte w zadaniu
                foreach (var usedPart in task.UsedParts)
                {
                    gfx.DrawString($"Część: {usedPart.Part?.Name} - Ilość: {usedPart.Quantity} - Cena: {usedPart.Part?.UnitPrice * usedPart.Quantity} PLN", font, XBrushes.Black, new XPoint(20, yPoint));
                    yPoint += 20;
                }
            }

            yPoint += 20;  // Dodajemy odstęp po każdym zleceniu
        }

        // Zapisz dokument PDF do strumienia pamięci
        using (var memoryStream = new MemoryStream())
        {
            document.Save(memoryStream, false);
            memoryStream.Seek(0, SeekOrigin.Begin);
            return File(memoryStream.ToArray(), "application/pdf", "Raport_zlecen.pdf");
        }
    }

        
        public async Task<IActionResult> Dashboard()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    // Jeśli użytkownik nie jest zalogowany, przekieruj go do strony logowania.
                    return RedirectToAction("Login", "Account");
                }

                var customer = await _context.Customers.FirstOrDefaultAsync(c => c.UserId == user.Id);
                if (customer == null)
                {
                    // Jeśli klient nie został znaleziony, przekieruj do formularza tworzenia klienta.
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
            catch (Exception ex)
            {
                // Logowanie błędu w przypadku wystąpienia problemu
                _logger.LogError($"Błąd podczas ładowania dashboardu: {ex.Message}");
                TempData["Error"] = "Wystąpił błąd podczas ładowania danych.";
                return RedirectToAction("Index", "Home");
            }
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
                try
                {
                    _context.Add(vehicle);
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Dashboard", "Client");
                }
                catch (Exception ex)
                {
                    // Logowanie błędów przy zapisie danych
                    _logger.LogError($"Błąd podczas dodawania pojazdu: {ex.Message}");
                    TempData["Error"] = "Nie udało się dodać pojazdu. Spróbuj ponownie.";
                }
            }
            // Przekazanie błędów walidacji do widoku, jeśli model nie jest poprawny
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

            var vehicle = await _context.Vehicles.FirstOrDefaultAsync(v => v.Id == VehicleId && v.CustomerId == customer.Id);
            if (vehicle == null)
            {
                TempData["Error"] = "Nie znaleziono pojazdu.";
                return RedirectToAction("Dashboard", "Client");
            }

            var order = new ServiceOrder
            {
                VehicleId = VehicleId,
                CreatedAt = DateTime.Now,
                Status = "Nowe"
            };

            if (ModelState.IsValid)
            {
                try
                {
                    _context.ServiceOrders.Add(order);
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Dashboard", "Client");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Błąd podczas dodawania zlecenia: {ex.Message}");
                    TempData["Error"] = "Nie udało się dodać zlecenia. Spróbuj ponownie.";
                }
            }

            // W przypadku błędu, przekaż ponownie pojazdy do widoku
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
    }
}
