using backend.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace backend.Data;

public class AppDbContext: DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options): base(options){}

    public DbSet<Category> Categories => Set<Category>();
    
    public DbSet<User> Users => Set<User>();
    
    public DbSet<Payment> Payments => Set<Payment>();
    
    public DbSet<RecPayment> RecPayments => Set<RecPayment>();
}