using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NetKubernetes.Controllers;
using NetKubernetes.Models;

namespace NetKubernetes.Data;

public class AppDbContext : IdentityDbContext<Usuario>
{
    public AppDbContext(DbContextOptions<AppDbContext> opt) : base(opt)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.Entity<Usuario>().Property(x => x.Id).HasMaxLength(36);
        builder.Entity<Usuario>().Property(x => x.NormalizedUserName).HasMaxLength(90);
        builder.Entity<IdentityRole>().Property(x => x.Id).HasMaxLength(36);
        builder.Entity<IdentityRole>().Property(x => x.NormalizedName).HasMaxLength(90);
    }

    public DbSet<Inmueble>? Inmuebles { get; set; }

}