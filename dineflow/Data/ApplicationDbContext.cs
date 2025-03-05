    using dineflow.Models;
    using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.AspNetCore.Identity;
    using dineflow.ViewModel;

    namespace dineflow.Data
    {
        public class ApplicationDbContext : IdentityDbContext
        {
            public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
                : base(options)
            {
            }

            public DbSet<Menu> Menus { get; set; }
            public DbSet<Category> Categories { get; set; } // ✅ Added Category
            public DbSet<Transaction> Transactions { get; set; }
            public DbSet<TransactionDetail> TransactionDetails { get; set; }
            public DbSet<Reservation> Reservations { get; set; }
            public DbSet<ApplicationUser> ApplicationUsers { get; set; }
            public DbSet<Inventory> Inventories { get; set; }
            public DbSet<InventoryDetails> InventoryDetails { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                base.OnModelCreating(modelBuilder);

                // Category & Menu Relationship
                modelBuilder.Entity<Menu>()
                    .HasOne(m => m.Category)
                    .WithMany(c => c.Menus)
                    .HasForeignKey(m => m.CategoryId)
                    .OnDelete(DeleteBehavior.Cascade);

                // TransactionDetail & Menu Relationship
                modelBuilder.Entity<TransactionDetail>()
                    .HasOne(td => td.MenuItem)
                    .WithMany()
                    .HasForeignKey(td => td.MenuItemId);

                // TransactionDetail & Transaction Relationship
                modelBuilder.Entity<TransactionDetail>()
                    .HasOne(td => td.Transaction)
                    .WithMany(t => t.TransactionDetails)
                    .HasForeignKey(td => td.TransactionId);

                modelBuilder.Entity<InventoryDetails>()
                    .HasOne(id => id.Inventory)
                    .WithMany(i => i.InventoryDetails)
                    .HasForeignKey(id => id.InventoryId)
                    .OnDelete(DeleteBehavior.Cascade);

                  

            modelBuilder.Entity<IdentityUserLogin<string>>().HasKey(l => new { l.LoginProvider, l.ProviderKey });

            }
            
        }
    }
