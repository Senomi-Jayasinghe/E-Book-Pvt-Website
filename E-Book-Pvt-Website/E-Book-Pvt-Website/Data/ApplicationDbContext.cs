using E_Book_Pvt_Website.Models;
using Microsoft.EntityFrameworkCore;

namespace E_Book_Pvt_Website.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        // Define DbSets for your tables
        public DbSet<Book> Book { get; set; }
        public DbSet<Customer> Customer { get;}
    }
}
