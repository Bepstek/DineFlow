using dineflow.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using dineflow.ViewModel;
using Microsoft.AspNetCore.Identity;

namespace dineflow.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        
        public DbSet<Menu> Menus { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<TransactionDetail> TransactionDetails { get; set; }
        public DbSet<Reservation> Reservations { get; set; }

        public DbSet<ApplicationUser> ApplicationUsers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); // Ensure Identity tables are configured

            modelBuilder.Entity<TransactionDetail>()
                .HasOne(td => td.MenuItem)
                .WithMany()
                .HasForeignKey(td => td.MenuItemId);

            // Ensure IdentityUserLogin has a key
            modelBuilder.Entity<IdentityUserLogin<string>>().HasKey(l => new { l.LoginProvider, l.ProviderKey });
        }

    }
}
