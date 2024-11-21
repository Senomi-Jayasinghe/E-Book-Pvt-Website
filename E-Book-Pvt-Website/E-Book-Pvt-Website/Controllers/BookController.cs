using E_Book_Pvt_Website.Data;
using E_Book_Pvt_Website.Models;
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

        public async Task<IActionResult> Books()
        {
            // Retrieve all books from the database
            var books = await _context.Book.ToListAsync();

            // Pass the list of books to the view
            return View(books);
        }

        public IActionResult AddBooks()
        {
            return View(new Book());
        }

        public async Task<IActionResult> BrowseBooks()
        {
            var items = await _context.Book.ToListAsync();
            return View(items);
        }

        [HttpPost]
        public async Task<IActionResult> AddBooks(Book book, IFormFile bookImage)
        {
            if (ModelState.IsValid)
            {
                if (bookImage != null && bookImage.Length > 0)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        await bookImage.CopyToAsync(memoryStream);
                        book.book_image = memoryStream.ToArray(); // Store the image as byte array
                    }
                }

                // Save the book to the database
                _context.Book.Add(book);
                await _context.SaveChangesAsync();
                return RedirectToAction("Books");
            }

            return View(book);
        }

    }
}
