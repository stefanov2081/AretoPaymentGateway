using AretoPaymentGateway.Infrastructure.Persistence.EfCore.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AretoPaymentGateway.Infrastructure.Persistence.EfCore
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole<int>, int>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Card> Cards { get; set; }
        public DbSet<Payment> Payments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Payment>()
                .HasOne(bookmark => bookmark.Card)
                .WithMany(card => card.Payments)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Card>()
                .HasMany(card => card.Payments)
                .WithOne(payment => payment.Card)
                .OnDelete(DeleteBehavior.ClientCascade);

            modelBuilder.Entity<ApplicationUser>()
                .HasData(new
                {
                    Id = 1,
                    UserName = "test@test.com",
                    NormalizedUserName = "TEST@TEST.COM",
                    Email = "test@test.com",
                    EmailConfirmed = true,
                    PasswordHash = "AQAAAAEAACcQAAAAEE+zI+1OVuLxtdD19yyEsJ80j4yHZaMKB/qACMtAV6knfETdljCZiOS/lCXFFufmxA==", // qwerty
                    SecurityStamp = "KXIMXB6NZD3GEKYA6CCWK55UXFQ2HO7B",
                    ConcurrencyStamp = "0c5c6a24-7c88-42c2-bf71-6293629bee24",
                    PhoneNumberConfirmed = false,
                    TwoFactorEnabled = false,
                    LockoutEnabled = true,
                    AccessFailedCount = 0
                });
        }
    }
}
