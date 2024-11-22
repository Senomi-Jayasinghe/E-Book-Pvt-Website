using E_Book_Pvt_Website.Data;
using E_Book_Pvt_Website.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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
            var books = await _context.Book.ToListAsync();

            // Fetch authors as a dictionary of author IDs to names
            var authors = await _context.Author.ToDictionaryAsync(a => a.author_id, a => a.author_name);

            // Pass the dictionary to the view using ViewBag
            ViewBag.AuthorNames = authors;

            return View(books);
        }

        public IActionResult AddBooks()
        {
            // Assuming you have a DbContext instance named `_context`
            var authors = _context.Author
                                  .Select(a => new { a.author_id, a.author_name })
                                  .ToList();

            ViewBag.AuthorList = new SelectList(authors, "author_id", "author_name");
            return View(new Book());
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
        public async Task<IActionResult> BookDetails(int id)
        {
            var book = _context.Book.FirstOrDefault(b => b.book_id == id);
            if (book == null)
            {
                return NotFound();
            }

            var authors = _context.Author.ToDictionary(a => a.author_id, a => a.author_name);

            // Pass authors to the view
            ViewBag.AuthorNames = authors;

            // Convert the image to a base64 string if it exists
            if (book.book_image != null)
            {
                var base64Image = Convert.ToBase64String(book.book_image);
                ViewData["ImageBase64"] = $"data:image/jpeg;base64,{base64Image}";
            }

            return View(book);
        }

        // GET: EditBooks
        public IActionResult EditBooks(int id)
        {
            // Get the list of authors from the database
            var authors = _context.Author
                                  .Select(a => new { a.author_id, a.author_name })
                                  .ToList();

            // Create the SelectList for dropdown options
            ViewBag.AuthorNames = new SelectList(authors, "author_id", "author_name");

            // Get the book to edit
            var book = _context.Book.FirstOrDefault(b => b.book_id == id);

            // Convert the book image to a base64 string if it exists
            if (book?.book_image != null)
            {
                ViewBag.ImageBase64 = $"data:image/jpeg;base64,{Convert.ToBase64String(book.book_image)}";
            }

            return View(book);
        }


        // POST: EditBooks
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditBooks(int id, Book book, IFormFile bookImage)
        {
            if (id != book.book_id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Get the existing book record
                    var existingBook = _context.Book.FirstOrDefault(b => b.book_id == id);
                    if (existingBook == null)
                    {
                        return NotFound();
                    }

                    // Update book details
                    existingBook.book_title = book.book_title;
                    existingBook.book_description = book.book_description;
                    existingBook.book_publisher = book.book_publisher;
                    existingBook.book_price = book.book_price;
                    existingBook.book_ISBN = book.book_ISBN;
                    existingBook.book_author_id = book.book_author_id;

                    // If a new image is uploaded, replace the existing one
                    if (bookImage != null && bookImage.Length > 0)
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            await bookImage.CopyToAsync(memoryStream);
                            existingBook.book_image = memoryStream.ToArray(); // Store image as byte array
                        }
                    }

                    // Save changes to the database
                    _context.Update(existingBook);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Book.Any(b => b.book_id == book.book_id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Books)); // Redirect to the list of books after editing
            }

            // If form submission fails, repopulate the SelectList for authors
            var authors = _context.Author
                                  .Select(a => new { a.author_id, a.author_name })
                                  .ToList();
            ViewBag.AuthorNames = new SelectList(authors, "author_id", "author_name");

            // Pass the current image back to the view if there was an error
            if (book.book_image != null)
            {
                ViewBag.ImageBase64 = $"data:image/jpeg;base64,{Convert.ToBase64String(book.book_image)}";
            }

            return View(book);
        }

        public async Task<IActionResult> BrowseBooks()
        {
            var items = await _context.Book.ToListAsync();
            return View(items);
        }

    }
}
