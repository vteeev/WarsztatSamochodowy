using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using worbench.Models;
using worbench.Services;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using worbench.Mappers;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;
using System;
using worbench.DTOs;

namespace worbench.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IAdminService _adminService;

        public AdminController(IAdminService adminService)
        {
            _adminService = adminService;
        }

        public async Task<IActionResult> Index()
        {
            var users = await _adminService.GetAllUsers();
            return View(users);
        }

        [HttpPost]
        public async Task<IActionResult> ChangeUserRole(string userId, string newRole)
        {
            var result = await _adminService.ChangeUserRole(userId, newRole);
            if (result.Message != null)
            {
                if (result.Message.StartsWith("Błąd") || result.Message.StartsWith("Rola"))
                    TempData["Error"] = result.Message;
                else
                    TempData["Success"] = result.Message;
            }
            return result.Result;
        }

        public async Task<IActionResult> Orders()
        {
            var orders = await _adminService.GetAllOrders();
            return View(orders);
        }

        public async Task<IActionResult> AssignMechanic(int id)
        {
            return await _adminService.GetAssignMechanicView(id);
        }

        [HttpPost]
        public async Task<IActionResult> AssignMechanic(int orderId, int mechanicId)
        {
            var result = await _adminService.AssignMechanic(orderId, mechanicId, ModelState);
            if (result.Message != null)
            {
                TempData["Success"] = result.Message;
            }
            return result.Result;
        }

        public async Task<IActionResult> GeneratePdfReport()
        {
            return await _adminService.GeneratePdfReport();
        }
    }
} 