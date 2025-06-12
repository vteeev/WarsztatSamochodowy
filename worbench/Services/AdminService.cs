using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using worbench.Models;
using worbench.Data;
using worbench.DTOs;
using worbench.Mappers;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;
using System;
using System.Linq;

namespace worbench.Services
{
    public class AdminService : IAdminService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly WorkshopDbContext _context;

        public AdminService(
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager,
            WorkshopDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }

        public async Task<IEnumerable<ApplicationUser>> GetAllUsers()
        {
            return await _userManager.Users.ToListAsync();
        }

        public async Task<(IActionResult Result, string Message)> ChangeUserRole(string userId, string newRole)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return (new NotFoundResult(), "Użytkownik nie został znaleziony.");

            var currentRoles = await _userManager.GetRolesAsync(user);
            var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
            if (!removeResult.Succeeded)
            {
                return (new RedirectToActionResult("Index", "Admin", null),
                    "Błąd podczas usuwania starych ról: " + string.Join(", ", removeResult.Errors.Select(e => e.Description)));
            }

            if (!await _roleManager.RoleExistsAsync(newRole))
            {
                return (new RedirectToActionResult("Index", "Admin", null),
                    $"Rola '{newRole}' nie istnieje.");
            }

            var addResult = await _userManager.AddToRoleAsync(user, newRole);
            if (!addResult.Succeeded)
            {
                return (new RedirectToActionResult("Index", "Admin", null),
                    "Błąd podczas dodawania roli: " + string.Join(", ", addResult.Errors.Select(e => e.Description)));
            }

            return (new RedirectToActionResult("Index", "Admin", null),
                "Rola została zmieniona.");
        }

        public async Task<IEnumerable<ServiceOrderDto>> GetAllOrders()
        {
            var orders = await _context.ServiceOrders
                .Include(o => o.Vehicle)
                .Include(o => o.AssignedMechanic)
                .ToListAsync();
            return orders.Select(ServiceOrderMapper.ToDtoWithCustom);
        }

        public async Task<IActionResult> GetAssignMechanicView(int orderId)
        {
            var order = await _context.ServiceOrders
                .Include(o => o.Vehicle)
                .FirstOrDefaultAsync(o => o.Id == orderId);
            if (order == null) return new NotFoundResult();

            var mechanics = await _userManager.GetUsersInRoleAsync("Mechanik");
            return new ViewResult
            {
                ViewName = "AssignMechanic",
                ViewData = new Microsoft.AspNetCore.Mvc.ViewFeatures.ViewDataDictionary<ServiceOrder>(
                    new Microsoft.AspNetCore.Mvc.ModelBinding.EmptyModelMetadataProvider(),
                    new ModelStateDictionary())
                {
                    Model = order,
                    ["Mechanics"] = mechanics
                }
            };
        }

        public async Task<(IActionResult Result, string Message)> AssignMechanic(int orderId, int mechanicId, ModelStateDictionary modelState)
        {
            var order = await _context.ServiceOrders.FirstOrDefaultAsync(o => o.Id == orderId);
            if (order == null) return (new NotFoundResult(), "Zlecenie nie zostało znalezione.");

            if (modelState.IsValid)
            {
                order.AssignedMechanicId = mechanicId;
                await _context.SaveChangesAsync();
                return (new RedirectToActionResult("Orders", "Admin", null),
                    "Mechanik został przypisany do zlecenia.");
            }

            return (new RedirectToActionResult("Orders", "Admin", null), "Wystąpił błąd podczas przypisywania mechanika.");
        }

        public async Task<IActionResult> GeneratePdfReport()
        {
            using (MemoryStream ms = new MemoryStream())
            {
                Document document = new Document(PageSize.A4, 25, 25, 30, 30);
                PdfWriter writer = PdfWriter.GetInstance(document, ms);
                document.Open();

                Font titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16);
                Paragraph title = new Paragraph("Raport zleceń serwisowych", titleFont);
                title.Alignment = Element.ALIGN_CENTER;
                title.SpacingAfter = 20f;
                document.Add(title);

                var orders = await _context.ServiceOrders
                    .Include(o => o.Vehicle)
                    .Include(o => o.AssignedMechanic)
                    .Include(o => o.ServiceTasks)
                        .ThenInclude(t => t.UsedParts)
                            .ThenInclude(p => p.Part)
                    .ToListAsync();

                foreach (var order in orders)
                {
                    Font orderFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12);
                    Paragraph orderInfo = new Paragraph($"Zlecenie #{order.Id}", orderFont);
                    orderInfo.SpacingBefore = 10f;
                    orderInfo.SpacingAfter = 5f;
                    document.Add(orderInfo);

                    Font normalFont = FontFactory.GetFont(FontFactory.HELVETICA, 10);
                    document.Add(new Paragraph($"Pojazd: {order.Vehicle?.Make} {order.Vehicle?.Model}", normalFont));
                    document.Add(new Paragraph($"Mechanik: {order.AssignedMechanic?.Email ?? "Nieprzypisany"}", normalFont));
                    document.Add(new Paragraph($"Status: {order.Status}", normalFont));
                    document.Add(new Paragraph($"Data utworzenia: {order.CreatedAt:dd.MM.yyyy}", normalFont));

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
                return new FileContentResult(bytes, "application/pdf")
                {
                    FileDownloadName = $"raport_zlecen_{DateTime.Now:yyyyMMdd}.pdf"
                };
            }
        }
    }
} 