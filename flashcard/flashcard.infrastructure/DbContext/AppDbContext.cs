using flashcard.infrastructure.IdentityEntities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace flashcard.infrastructure.DbContext;

public class AppDbContext:IdentityDbContext<User, Role, Guid>
{
    public AppDbContext(DbContextOptions options):base(options)
    {
        
    }
    public DbSet<RefreshToken> RefreshTokens { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<RefreshToken>().HasKey(x => x.Id);

        builder.Entity<RefreshToken>().HasIndex(x=>x.Token)
            .IsUnique();

        builder.Entity<RefreshToken>()
            .HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.UserId);
        base.OnModelCreating(builder);
    }
}
