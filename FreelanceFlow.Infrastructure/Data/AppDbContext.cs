using FreelanceFlow.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace FreelanceFlow.Infrastructure.Data;

public class AppDbContext : DbContext
{
    private readonly IConfiguration _configuration;

    public AppDbContext(DbContextOptions<AppDbContext> options, IConfiguration configuration) : base(options)
    {
        _configuration = configuration;
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Conversation> Conversations { get; set; }
    public DbSet<GeneratedDocument> GeneratedDocuments { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection") ?? "Data Source=freelanceflow.db";
            optionsBuilder.UseSqlite(connectionString);
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Conversation>()
            .HasOne(c => c.User)
            .WithMany()
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<GeneratedDocument>()
            .HasOne(d => d.Conversation)
            .WithMany(c => c.Documents)
            .HasForeignKey(d => d.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<User>().HasData(
            new User
            {
                Id = Guid.Parse("8f6cf95b-739e-4f98-b3ea-2f3d8f22dc51"),
                FullName = "Demo Kullanıcı",
                Email = "demo@freelanceflow.com",
                PasswordHash = "$2a$11$7EqJtq98hPqEX7fNZaFWoOhiBqQ3N5Qf5pY88MtXgFp0cNT6a8uGa",
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            }
        );
    }
}
