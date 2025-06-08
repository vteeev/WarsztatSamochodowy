namespace worbench.Data;
using worbench.Models;

using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
public class WorkshopDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, int>
{
    public WorkshopDbContext(DbContextOptions<WorkshopDbContext> options) : base(options) { }

    public DbSet<Customer> Customers { get; set; }
    public DbSet<Vehicle> Vehicles { get; set; }
    public DbSet<Part> Parts { get; set; }
    public DbSet<ServiceOrder> ServiceOrders { get; set; }
    public DbSet<ServiceTask> ServiceTasks { get; set; }
    public DbSet<UsedPart> UsedParts { get; set; }
    public DbSet<Comment> Comments { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Part>()
            .Property(p => p.UnitPrice)
            .HasPrecision(10, 2); // np. 10 cyfr, z czego 2 po przecinku

        modelBuilder.Entity<ServiceTask>()
            .Property(t => t.LaborCost)
            .HasPrecision(10, 2);

        // Naprawa: relacja Comment -> ServiceOrder bez kaskadowego usuwania
        modelBuilder.Entity<Comment>()
            .HasOne(c => c.ServiceOrder)
            .WithMany(so => so.Comments)
            .HasForeignKey(c => c.ServiceOrderId)
            .OnDelete(DeleteBehavior.Restrict);

        // Unikalny indeks na UserId w Customer
        modelBuilder.Entity<Customer>()
            .HasIndex(c => c.UserId)
            .IsUnique();
    }

}