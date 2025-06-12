using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using worbench.Models;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using worbench.Mappers;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;
using System;

namespace worbench.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly worbench.Data.WorkshopDbContext _context;

        public AdminController(UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager, worbench.Data.WorkshopDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }

        public IActionResult Index()
        {
            var users = _userManager.Users.ToList();
            return View(users);
        }

        [HttpPost]
        public async Task<IActionResult> ChangeRole(string userId, string newRole)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            var currentRoles = await _userManager.GetRolesAsync(user);
            var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
            if (!removeResult.Succeeded)
            {
                TempData["Error"] = "Błąd podczas usuwania starych ról: " + string.Join(", ", removeResult.Errors.Select(e => e.Description));
                return RedirectToAction("Index");
            }

            // Sprawdź, czy rola istnieje
            if (!await _roleManager.RoleExistsAsync(newRole))
            {
                TempData["Error"] = $"Rola '{newRole}' nie istnieje.";
                return RedirectToAction("Index");
            }

            var addResult = await _userManager.AddToRoleAsync(user, newRole);
            if (!addResult.Succeeded)
            {
                TempData["Error"] = "Błąd podczas dodawania roli: " + string.Join(", ", addResult.Errors.Select(e => e.Description));
                return RedirectToAction("Index");
            }

            TempData["Success"] = "Rola została zmieniona.";
            return RedirectToAction("Index");
        }

        public IActionResult Orders()
        {
            var orders = _context.ServiceOrders.Include(o => o.Vehicle).Include(o => o.AssignedMechanic).ToList();
            var orderDtos = orders.Select(Mappers.ServiceOrderMapper.ToDtoWithCustom).ToList();
            return View(orderDtos);
        }

        [HttpGet]
        public IActionResult AssignMechanic(int orderId)
        {
            var order = _context.ServiceOrders.Include(o => o.Vehicle).FirstOrDefault(o => o.Id == orderId);
            if (order == null) return NotFound();
            var mechanics = _userManager.GetUsersInRoleAsync("Mechanik").Result.ToList();
            ViewBag.Mechanics = mechanics;
            return View(order);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignMechanic(int orderId, int mechanicId)
        {
            var order = _context.ServiceOrders.FirstOrDefault(o => o.Id == orderId);
            if (order == null) return NotFound();
            order.AssignedMechanicId = mechanicId;
            await _context.SaveChangesAsync();
            TempData["Success"] = "Mechanik został przypisany do zlecenia.";
            return RedirectToAction("Orders");
        }

        public IActionResult GeneratePdfReport()
        {
            using (MemoryStream ms = new MemoryStream())
            {
                Document document = new Document(PageSize.A4, 25, 25, 30, 30);
                PdfWriter writer = PdfWriter.GetInstance(document, ms);
                document.Open();

                // Add title
                Font titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16);
                Paragraph title = new Paragraph("Raport zleceń serwisowych", titleFont);
                title.Alignment = Element.ALIGN_CENTER;
                title.SpacingAfter = 20f;
                document.Add(title);

                // Get all service orders with related data
                var orders = _context.ServiceOrders
                    .Include(o => o.Vehicle)
                    .Include(o => o.AssignedMechanic)
                    .Include(o => o.ServiceTasks)
                        .ThenInclude(t => t.UsedParts)
                            .ThenInclude(p => p.Part)
                    .ToList();

                foreach (var order in orders)
                {
                    // Add order information
                    Font orderFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12);
                    Paragraph orderInfo = new Paragraph($"Zlecenie #{order.Id}", orderFont);
                    orderInfo.SpacingBefore = 10f;
                    orderInfo.SpacingAfter = 5f;
                    document.Add(orderInfo);

                    // Add order details
                    Font normalFont = FontFactory.GetFont(FontFactory.HELVETICA, 10);
                    document.Add(new Paragraph($"Pojazd: {order.Vehicle?.Make} {order.Vehicle?.Model}", normalFont));
                    document.Add(new Paragraph($"Mechanik: {order.AssignedMechanic?.Email ?? "Nieprzypisany"}", normalFont));
                    document.Add(new Paragraph($"Status: {order.Status}", normalFont));
                    document.Add(new Paragraph($"Data utworzenia: {order.CreatedAt:dd.MM.yyyy}", normalFont));

                    // Add service tasks
                    if (order.ServiceTasks.Any())
                    {
                        document.Add(new Paragraph("Zadania serwisowe:", normalFont));
                        PdfPTable taskTable = new PdfPTable(3);
                        taskTable.WidthPercentage = 100;
                        taskTable.AddCell("Opis");
                        taskTable.AddCell("Koszt robocizny");
                        taskTable.AddCell("Części");

                        foreach (var task in order.ServiceTasks)
                        {
                            taskTable.AddCell(task.Description);
                            taskTable.AddCell(task.LaborCost.ToString("C"));
                            taskTable.AddCell(string.Join(", ", task.UsedParts.Select(p => p.Part.Name)));
                        }
                        document.Add(taskTable);
                    }

                    document.Add(new Paragraph("\n"));
                }

                document.Close();
                byte[] bytes = ms.ToArray();
                return File(bytes, "application/pdf", $"raport_zlecen_{DateTime.Now:yyyyMMdd}.pdf");
            }
        }
    }
} 