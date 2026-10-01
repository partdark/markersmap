using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Data
{
    public class MarkersDBContext : IdentityDbContext<UserEntity, IdentityRole<Guid>, Guid>
    {
        public MarkersDBContext(DbContextOptions options) : base(options)
        {

        }

        public virtual DbSet<CategoryEntity> Category { get; set; }

        public virtual DbSet<ContentEntity> Content { get; set; }

        public virtual DbSet<MarkerEntity> Marker { get; set; }

        public virtual DbSet<UserEntity> User { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<CategoryEntity>(c =>
            {
                c.ToTable("category");
                c.HasKey(c => c.Id);
                c.HasMany(m => m.Markers).WithOne(c => c.Category);
                c.Property(c => c.Name).HasMaxLength(40);
                c.HasIndex(c => c.Name).IsUnique();

            });

            builder.Entity<ContentEntity>(c =>
            {
                c.ToTable("content");
                c.HasKey(c => c.Id);
                c.HasOne(m => m.Marker).WithMany(c => c.Content)
                .HasForeignKey(c => c.MarkerId)
                .OnDelete(DeleteBehavior.Cascade);

            });

            builder.Entity<MarkerEntity>(m =>
            {
                m.ToTable("markers");
                m.HasKey(m => m.Id);
                m.Property(m => m.Description).HasMaxLength(200);
                
                m.HasOne(c => c.Category).WithMany(m => m.Markers)
                .HasForeignKey(c => c.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

                m.HasOne(m => m.User).WithMany(m => m.Markers)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);

                m.HasMany(m => m.Content).WithOne(m => m.Marker).HasForeignKey(m => m.MarkerId)
                .OnDelete(DeleteBehavior.Cascade);

                m.HasIndex(m => m.CategoryId);
                m.HasIndex(m => m.UserId);
            }
            );
        }
    }
}
