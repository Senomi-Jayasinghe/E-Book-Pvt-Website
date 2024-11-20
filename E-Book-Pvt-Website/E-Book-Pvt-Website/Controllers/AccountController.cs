using E_Book_Pvt_Website.Data;
using E_Book_Pvt_Website.Models;
using Microsoft.AspNetCore.Mvc;

namespace E_Book_Pvt_Website.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult CustomerLogin()
        {
            return View(new CustomerLoginModal());
        }
        [HttpPost]
        public IActionResult CustomerLogin(CustomerLoginModal loginModel)
        {
            if (ModelState.IsValid)
            {
                // Check credentials
                bool isAuthenticated = AuthenticateCustomer(loginModel.Email, loginModel.Password);

                if (isAuthenticated)
                {
                    // Authentication successful
                    TempData["Message"] = "Login successful!";
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    // Invalid credentials
                    ModelState.AddModelError("", "Invalid email or password.");
                }
            }
            else
            {
                ModelState.AddModelError("", "Email is required.");
            }

            return View(loginModel);
        }

        private bool AuthenticateCustomer(string email, string password)
        {
            // Ensure email is not null or empty before querying
            if (string.IsNullOrWhiteSpace(email))
                return false;

            // Find the customer in the database
            var customer = _context.Customer
                .FirstOrDefault(c => c.customer_email == email);

            if (customer != null)
            {
                // Verify password (assuming stored passwords are hashed)
                return VerifyPassword(password, customer.customer_password);
            }

            return false; // Customer not found
        }


        private bool VerifyPassword(string inputPassword, byte[] storedPassword)
        {
            // Example: Convert the input password to hash and compare
            // Replace with your hashing logic (e.g., using BCrypt, SHA256)
            var inputPasswordBytes = System.Text.Encoding.UTF8.GetBytes(inputPassword);
            return inputPasswordBytes.SequenceEqual(storedPassword);
        }
    }
}
