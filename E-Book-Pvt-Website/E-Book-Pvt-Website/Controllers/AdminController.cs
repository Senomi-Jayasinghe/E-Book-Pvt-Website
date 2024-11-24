using E_Book_Pvt_Website.Data;
using E_Book_Pvt_Website.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using static System.Reflection.Metadata.BlobBuilder;
using System.Text.Json; // Ensure System.Text.Json is imported for Json serialization

namespace E_Book_Pvt_Website.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> AdminList()
        {
            var admins = await _context.Admin.ToListAsync();
            return View(admins);
        }

        public IActionResult CreateAdmin()
        {
            return View(new Admin());
        }

        [HttpPost]
        public async Task<IActionResult> CreateAdmin(Admin admin)
        {
            if (ModelState.IsValid)
            {
                _context.Admin.Add(admin);
                await _context.SaveChangesAsync();
                return RedirectToAction("AdminList");
            }

            return View(admin);
        }

        public IActionResult EditAdmin(int id)
        {
            // Get the admin to edit
            var admin = _context.Admin.FirstOrDefault(b => b.admin_id == id);
            return View(admin);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditAdmin(int id, Admin admin)
        {
            if (id != admin.admin_id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Get the existing admin record
                    var existingAdmin = _context.Admin.FirstOrDefault(b => b.admin_id == id);
                    if (existingAdmin == null)
                    {
                        return NotFound();
                    }

                    // Update admin details
                    existingAdmin.admin_name = admin.admin_name;
                    existingAdmin.admin_phoneno = admin.admin_phoneno;
                    existingAdmin.admin_email = admin.admin_email;
                    existingAdmin.admin_password = admin.admin_password;

                    // Save changes to the database
                    _context.Update(existingAdmin);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Admin.Any(b => b.admin_id == admin.admin_id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(AdminList)); // Redirect to the list of books after editing
            }
            return View(admin);
        }

        public IActionResult AdminDetails(int id)
        {
            // Get the admin to edit
            var admin = _context.Admin.FirstOrDefault(b => b.admin_id == id);
            return View(admin);
        }

        [HttpGet]
        public IActionResult DeleteAdmin(int id)
        {
            // Get the admin to edit
            var admin = _context.Admin.FirstOrDefault(b => b.admin_id == id);
            return View(admin);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAdminConfirmed(int id)
        {
            // Retrieve the admin by admin_id
            var admin = await _context.Admin.FindAsync(id);

            if (admin == null)
            {
                return NotFound(); // Return 404 if admin not found
            }

            // Delete the admin from the database
            _context.Admin.Remove(admin);

            // Save changes to apply the deletion
            await _context.SaveChangesAsync();

            // Redirect to the admin list page after deletion
            return RedirectToAction(nameof(AdminList));
        }



        public async Task<IActionResult> AdminDashboard()
        {
            // Orders summary by status for pie chart
            var orderStatuses = await _context.Order
                .GroupBy(o => o.order_status)
                .Select(g => new
                {
                    Status = g.Key,
                    Count = g.Count()
                })
                .ToListAsync();

            // Prepare data for the chart
            ViewBag.StatusLabels = JsonSerializer.Serialize(orderStatuses.Select(s => s.Status).ToList());
            ViewBag.StatusData = JsonSerializer.Serialize(orderStatuses.Select(s => s.Count).ToList());

            // Total number of orders placed
            ViewBag.TotalOrders = await _context.Order.CountAsync();

            // Total number of books
            ViewBag.TotalBooks = await _context.Book.CountAsync();

            // Total revenue (sum of order prices)
            ViewBag.TotalRevenue = await _context.Order.SumAsync(o => o.order_price);

            return View();
        }

    }
}
