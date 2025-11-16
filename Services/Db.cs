using ClipBoard.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;

namespace ClipBoard.Services
{
    public class Db : DbContext
    {
        public Db(DbContextOptions<Db> options) : base(options)
        {
        }

        public DbSet<ClipGroupRecord> ClipGroups => Set<ClipGroupRecord>();
        public DbSet<ClipRecord> Clips => Set<ClipRecord>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ClipGroupRecord>(g =>
            {
                g.ToTable("ClipGroups");
                g.HasKey(g => g.Id);

                g
                .Property(g => g.Id)
                .ValueGeneratedOnAdd();

                g
                .HasMany(g => g.Clips)
                .WithOne(c => c.ClipGroup)
                .HasForeignKey(c => c.ClipGroupId)
                .OnDelete(DeleteBehavior.Cascade); // When a group is deleted, delete its clips

                g.Property(g => g.SortOrder)
                .IsRequired();
            });

            modelBuilder.Entity<ClipRecord>(c =>
            {
                c.ToTable("Clips");
                c.HasIndex(c => new { c.ClipGroupId, c.SortOrder });

                c
                .Property(c => c.Id)
                .ValueGeneratedOnAdd();

                c.Property(c => c.SortOrder)
                .IsRequired();
            });
        }
    }
}
