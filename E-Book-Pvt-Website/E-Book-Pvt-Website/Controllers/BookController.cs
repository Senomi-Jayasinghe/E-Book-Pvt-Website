using E_Book_Pvt_Website.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace E_Book_Pvt_Website.Controllers
{
    public class BookController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BookController(ApplicationDbContext context)
        {
            _context = context;
        }
        public IActionResult Books()
        {
            return View();
        }

        public async Task<IActionResult> BrowseBooks()
        {
            var items = await _context.Book.ToListAsync();
            return View(items);
        }
    }
}
