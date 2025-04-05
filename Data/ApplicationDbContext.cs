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
        public DbSet<Material_Request> Material_Requests { get; set; }
        public DbSet<Material_Request_Item> Material_Request_Item { get; set; }
        public DbSet<Transaction_Items> Transaction_Items { get; set; }
        public DbSet<UserActivityLog> UserActivityLogs { get; set; }
   
        public DbSet<Material_Consumption> UserAMaterial_ConsumptionctivityLogs { get; set; }
        public DbSet<Material_Consumption_Item> Material_Consumption_Item { get; set; }
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

            modelBuilder.Entity<Material_Consumption_Item>()
              .HasOne(m => m.Unit)
              .WithMany(u => u.Material_Consumption_Item)
              .HasForeignKey(m => m.unit_id)
              .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Material_Consumption_Item>()
              .HasOne(m => m.Material)
              .WithMany(u => u.Material_Consumption_Item)
              .HasForeignKey(m => m.material_id)
              .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Material_Consumption>()
              .HasMany(mc => mc.Material_Consumption_Item)
              .WithOne(item => item.Material_Consumption)
              .HasForeignKey(item => item.consumption_id)
              .OnDelete(DeleteBehavior.Cascade);


            modelBuilder.Entity<Material_Request>()
            .HasMany(mr => mr.Material_Request_Item)
            .WithOne(i => i.material_request)
            .HasForeignKey(i => i.request_id)
            .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Material_Request>()
            .HasOne(m => m.Site)
            .WithMany(u => u.Material_Request)
            .HasForeignKey(m => m.site_id)
            .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Material_Consumption>()
            .HasOne(m => m.Site)
            .WithMany(u => u.Material_Consumption)
            .HasForeignKey(m => m.site_id)
            .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Material_Request_Item>()
              .HasOne(m => m.Material)
              .WithMany(u => u.Material_Request_Item)
              .HasForeignKey(m => m.material_id)
              .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Material_Request_Item>()
            .HasOne(m => m.Unit)
            .WithMany(u => u.Material_Request_Item)
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

            modelBuilder.Entity<Transaction_Items>()
           .HasOne(m => m.Material)
           .WithMany(u => u.Transaction_Items)
           .HasForeignKey(m => m.material_id)
           .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Transaction_Items>()
            .HasOne(m => m.Unit)
            .WithMany(u => u.Transaction_Items)
            .HasForeignKey(m => m.unit_type_id)
            .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Transaction>()
              .HasMany(t => t.TransactionItems)
              .WithOne(ti => ti.Transaction)
              .HasForeignKey(ti => ti.transaction_id)
              .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Transaction>()
            .HasOne(m => m.Supplier)
            .WithMany(u => u.Transactions)
            .HasForeignKey(m => m.form_supplier_id)
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
