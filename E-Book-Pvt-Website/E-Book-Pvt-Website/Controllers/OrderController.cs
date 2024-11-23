using E_Book_Pvt_Website.Data;
using E_Book_Pvt_Website.Helpers;
using E_Book_Pvt_Website.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace E_Book_Pvt_Website.Controllers
{
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OrderController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public IActionResult Checkout(double totalPrice)
        {
            // Pass the total price to the checkout view
            var viewModel = new OrderViewModel
            {
                TotalPrice = totalPrice
            };
            return View(viewModel);
        }

        [HttpPost]
        public IActionResult PlaceOrder(OrderViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Retrieve the customer ID from the session
                int? customerId = HttpContext.Session.GetInt32("customer_id");
                if (customerId == null) return RedirectToAction("CustomerLogin", "Account");

                // Insert into Order table
                var order = new Order
                {
                    order_customer_id = customerId.Value,
                    order_price = model.TotalPrice,
                    order_date = model.OrderDate,
                    order_address = model.OrderAddress,
                    order_phoneno = model.OrderPhoneNo,
                    order_status = "Placed"
                };

                _context.Order.Add(order);
                _context.SaveChanges();

                // Retrieve cart items from the session or database (assuming session here)
                var cartItems = GetCartItems();

                // Insert into OrderBook table
                foreach (var cartItem in cartItems)
                {
                    var orderBook = new OrderBook
                    {
                        order_id = order.order_id,      // Use the generated Order ID
                        book_id = cartItem.Book.book_id // Each book in the cart
                    };

                    _context.OrderBook.Add(orderBook);
                }

                _context.SaveChanges();

                // Clear the cart after placing the order
                ClearCart();

                return RedirectToAction("OrderConfirmation");
            }

            return View("Checkout", model); // Show errors if validation fails
        }

        private List<CartItem> GetCartItems()
        {
            // Retrieve cart items from session or database
            // Example for session-based cart
            return HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();
        }

        private void ClearCart()
        {
            // Clear cart items from session
            HttpContext.Session.Remove("Cart");
        }

        public IActionResult OrderConfirmation()
        {
            return View();
        }

        public IActionResult ViewOrders()
        {
            // Retrieve role_id and customer_id from session
            int? roleId = HttpContext.Session.GetInt32("role_id");
            int? customerId = HttpContext.Session.GetInt32("customer_id");

            // Get the orders from the database
            IEnumerable<Order> orders;

            if (roleId == 1 && customerId.HasValue)
            {
                // If role_id is 1, show only the orders for the logged-in customer
                orders = _context.Order.Where(o => o.order_customer_id == customerId.Value).ToList();
            }
            else
            {
                // If role_id is not 1, show all orders
                orders = _context.Order.ToList();
            }

            return View(orders);
        }

        public async Task<IActionResult> OrderDetails(int orderId)
        {
            var order = await _context.Order.FirstOrDefaultAsync(o => o.order_id == orderId);
            if (order == null) return NotFound();

            // Retrieve the author names as a dictionary
            var authors = await _context.Author.ToDictionaryAsync(a => a.author_id, a => a.author_name);
            ViewBag.AuthorNames = authors;

            // Fetch order books along with book details
            var orderBooks = await _context.OrderBook
                .Where(ob => ob.order_id == orderId)
                .Select(ob => new OrderBookDetails
                {
                    Quantity = ob.quantity,
                    BookTitle = _context.Book.Where(b => b.book_id == ob.book_id).Select(b => b.book_title).FirstOrDefault(),
                    ISBN = _context.Book.Where(b => b.book_id == ob.book_id).Select(b => b.book_ISBN).FirstOrDefault(),
                    Image = _context.Book.Where(b => b.book_id == ob.book_id).Select(b => b.book_image).FirstOrDefault(),
                    AuthorId = _context.Book.Where(b => b.book_id == ob.book_id).Select(b => b.book_author_id).FirstOrDefault()
                })
                .ToListAsync();

            var viewModel = new OrderDetailsViewModel
            {
                Order = order,
                OrderBooks = orderBooks
            };

            return View(viewModel);
        }
    }
}
