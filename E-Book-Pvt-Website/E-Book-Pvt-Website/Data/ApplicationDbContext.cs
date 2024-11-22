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
        public DbSet<Customer> Customer { get; set; }
        public DbSet<Admin> Admin { get; set; }
        public DbSet<Author> Author { get; set; }
        public DbSet<BookCategory> BookCategory { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<BookCategory>()
                .HasKey(bc => new { bc.category_id, bc.book_id }); // Define composite primary key
            modelBuilder.Entity<OrderBook>()
                .HasKey(bc => new { bc.order_id, bc.book_id }); // Define composite primary key
            modelBuilder.Entity<BookFeedback>()
                .HasKey(bc => new { bc.book_id, bc.customer_id, bc.feedback }); // Define composite primary key
        }

        public DbSet<Category> Category { get; set; }
        public DbSet<Order> Order { get; set; }
        public DbSet<OrderBook> OrderBook { get; set; }
        public DbSet<BookFeedback> BookFeedback { get; set; }

    }
}
