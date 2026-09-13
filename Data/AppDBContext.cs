using Ferreteria.Models;
using Microsoft.EntityFrameworkCore;

namespace Ferreteria.Data;

public class AppDBContext : DbContext
{
    
    public AppDBContext(DbContextOptions<AppDBContext> options) : base(options)
    {
    }
    public DbSet<Product> Products { get; set; } = null!;
}