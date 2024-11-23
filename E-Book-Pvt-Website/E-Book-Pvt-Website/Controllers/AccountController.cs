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
                    // Get customer_id based on the authenticated user
                    var customer = _context.Customer.FirstOrDefault(c => c.customer_email == loginModel.Email);

                    if (customer != null)
                    {
                        // Store customer_id and role_id in session
                        HttpContext.Session.SetInt32("customer_id", customer.customer_id);
                        HttpContext.Session.SetInt32("role_id", 2); // Set role_id as 1

                        TempData["Message"] = "Login successful!";
                        return RedirectToAction("BrowseBooks", "Book");
                    }
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
                // Check if the entered password matches the stored password
                return customer.customer_password.Trim() == password.Trim();
            }

            return false; // Customer not found or password does not match
        }

        private bool VerifyPassword(string inputPassword, byte[] storedPassword)
        {
            // Example: Convert the input password to hash and compare
            // Replace with your hashing logic (e.g., using BCrypt, SHA256)
            var inputPasswordBytes = System.Text.Encoding.UTF8.GetBytes(inputPassword);
            return inputPasswordBytes.SequenceEqual(storedPassword);
        }

        // GET: SignUp page
        [HttpGet]
        public IActionResult SignUp()
        {
            return View(new CustomerSignUpViewModel());
        }

        // POST: Sign up new customer
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SignUp(CustomerSignUpViewModel model)
        {
            if (model.Password != model.ConfirmPassword)
            {
                ModelState.AddModelError("", "The password and confirmation password do not match.");
            }
            else
            {
                var customer = new Customer
                {
                    customer_name = model.Name,
                    customer_email = model.Email,
                    customer_phoneno = model.PhoneNumber,
                    customer_address = model.Address,
                    customer_password = model.Password // Hash password if necessary
                };

                _context.Customer.Add(customer);
                _context.SaveChanges();
                TempData["Message"] = "Registration successful!";
                return RedirectToAction("CustomerLogin");
            }

            // If model state is invalid, return the same view with validation errors
            return View(model);
        }

    }
}
