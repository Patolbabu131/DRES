using DRES.Models;
using Microsoft.EntityFrameworkCore;

namespace DRES.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options): base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Site> Sites { get; set; }
        public DbSet<Unit> Units { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }

        public DbSet<Material> Materials { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<Stock> Stocks { get; set; }
        public DbSet<UserActivityLog> UserActivityLogs { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Material>()
           .HasIndex(p => p.material_name)
           .IsUnique();

            modelBuilder.Entity<Site>(entity =>
            {
                entity.HasMany(s => s.Users)
                      .WithOne(u => u.Site)
                      .HasForeignKey(u => u.siteid)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Material>()
               .HasOne(m => m.Unit)
               .WithMany(u => u.Materials)
               .HasForeignKey(m => m.unit_id)
               .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Stock>()
              .HasOne(m => m.Material)
              .WithMany(u => u.Stocks)
              .HasForeignKey(m => m.material_id)
              .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Stock>()
             .HasOne(m => m.Site)
             .WithMany(u => u.Stocks)
             .HasForeignKey(m => m.site_id)
             .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Stock>()
             .HasOne(m => m.Unit)
             .WithMany(u => u.Stocks)
             .HasForeignKey(m => m.unit_type_id)
             .OnDelete(DeleteBehavior.Restrict);
                                                                        
            modelBuilder.Entity<Stock>()
           .HasOne(m => m.User)
           .WithMany(u => u.Stocks)
           .HasForeignKey(m => m.user_id)
           .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Transaction>()
           .HasOne(m => m.Material)
           .WithMany(u => u.Transactions)
           .HasForeignKey(m => m.material_id)
           .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Transaction>()
            .HasOne(m => m.Unit)
            .WithMany(u => u.Transactions)
            .HasForeignKey(m => m.unit_type_id)
            .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Transaction>()
            .HasOne(m => m.Supplier)
            .WithMany(u => u.Transactions)
            .HasForeignKey(m => m.form_supplier_id)
            .OnDelete(DeleteBehavior.Restrict);

              modelBuilder.Entity<Transaction>()
            .HasOne(m => m.user)
            .WithMany(u => u.Transactions)
            .HasForeignKey(m => m.from_userid)
            .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Transaction>()
            .HasOne(m => m.Site)
            .WithMany(u => u.Transactions)
            .HasForeignKey(m => m.from_site_id)
            .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Transaction>()
            .HasOne(m => m.user)
            .WithMany(u => u.Transactions)
            .HasForeignKey(m => m.to_user_id)
            .OnDelete(DeleteBehavior.Restrict);
            // Foreign key property

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasOne(u => u.Site)
                      .WithMany(s => s.Users)
                      .HasForeignKey(u => u.siteid)
                      .IsRequired(false)  // Since siteid is nullable
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Other configurations...
            modelBuilder.Entity<Site>()
                       .HasIndex(s => s.sitename)
                       .IsUnique();

            modelBuilder.Entity<Supplier>()
                       .HasIndex(s => s.gst)
                       .IsUnique();

            modelBuilder.Entity<Unit>()
                       .HasIndex(s => s.unitsymbol)
                       .IsUnique();
            modelBuilder.Entity<Unit>()
                       .HasIndex(s => s.unitname)
                       .IsUnique();

            modelBuilder.Entity<User>()
                     .HasIndex(u => u.username)
                     .IsUnique();
        }
    }


}
