using Microsoft.EntityFrameworkCore;
using BasicBlazor.Data.Models;

namespace BasicBlazor.Data.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Role entity
        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.RoleName)
                .IsRequired()
                .HasMaxLength(50);

            entity.HasIndex(e => e.RoleName)
                .IsUnique();
        });

        // Configure User entity
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Username)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.PasswordHash)
                .IsRequired()
                .HasMaxLength(500);

            entity.HasIndex(e => e.Username)
                .IsUnique();

            // Configure relationship: Many Users -> One Role
            entity.HasOne(e => e.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(e => e.RoleId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent cascade delete
        });

        // Configure Permission entity
        modelBuilder.Entity<Permission>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.PermissionName)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.Description)
                .HasMaxLength(500);

            entity.HasIndex(e => e.PermissionName)
                .IsUnique(); // Prevent duplicate permission names
        });

        // Configure RolePermission join entity
        modelBuilder.Entity<RolePermission>(entity =>
        {
            entity.HasKey(e => e.Id);

            // Prevent duplicate role-permission combinations
            entity.HasIndex(e => new { e.RoleId, e.PermissionId })
                .IsUnique();

            // Configure relationship: Many RolePermissions -> One Role
            entity.HasOne(e => e.Role)
                .WithMany(r => r.RolePermissions)
                .HasForeignKey(e => e.RoleId)
                .OnDelete(DeleteBehavior.Cascade); // Delete permissions when role deleted

            // Configure relationship: Many RolePermissions -> One Permission
            entity.HasOne(e => e.Permission)
                .WithMany(p => p.RolePermissions)
                .HasForeignKey(e => e.PermissionId)
                .OnDelete(DeleteBehavior.Cascade); // Delete mappings when permission deleted
        });

        // Seed data
        SeedData(modelBuilder);
    }

    private void SeedData(ModelBuilder modelBuilder)
    {
        // Seed Roles
        var roles = new[]
        {
            new Role { Id = 1, RoleName = "Admin" },
            new Role { Id = 2, RoleName = "Manager" },
            new Role { Id = 3, RoleName = "User" }
        };

        modelBuilder.Entity<Role>().HasData(roles);

        // Seed Users with BCrypt-hashed passwords (pre-computed for consistency)
        // These hashes were generated with BCrypt.Net.BCrypt.HashPassword() and hardcoded
        // to avoid non-deterministic model changes on each build
        var users = new[]
        {
            new User
            {
                Id = 1,
                Username = "admin",
                // Password: Admin123!
                PasswordHash = "$2a$11$58ak340eNjVB8EloR22cbe0s1vC7n.krMrU2Z9b9lbhgdht90yVFm",
                RoleId = 1 // Admin
            },
            new User
            {
                Id = 2,
                Username = "manager",
                // Password: Manager123!
                PasswordHash = "$2a$11$mXDsJNvh13nBhc3bUCTGIu6Z.lpFq96uURvzUdY.6jcsV4egwwj3y",
                RoleId = 2 // Manager
            },
            new User
            {
                Id = 3,
                Username = "user",
                // Password: User123!
                PasswordHash = "$2a$11$UFJ4Omi7WT9MHQhCgjGLwets5hFQj9ccrweL8OX3JmU5SbFuYdx1K",
                RoleId = 3 // User
            }
        };

        modelBuilder.Entity<User>().HasData(users);

        // Seed Permissions
        var permissions = new[]
        {
            new Permission
            {
                Id = 1,
                PermissionName = "Page3:See_Button",
                Description = "Allows user to see the special button on Page 3"
            }
        };

        modelBuilder.Entity<Permission>().HasData(permissions);

        // Seed RolePermissions - Initially only Manager (RoleId=2) has Page3:See_Button (PermissionId=1)
        var rolePermissions = new[]
        {
            new RolePermission
            {
                Id = 1,
                RoleId = 2, // Manager role
                PermissionId = 1 // Page3:See_Button permission
            }
        };

        modelBuilder.Entity<RolePermission>().HasData(rolePermissions);
    }
}
