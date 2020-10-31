using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using API.Domain;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public DbSet<ShortUrl> ShortUrl { get; set; }
}