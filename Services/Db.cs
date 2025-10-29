using ClipBoard.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;

namespace ClipBoard.Services
{
    public class Db : DbContext
    {
        public DbSet<ClipGroupRecord> ClipGroups => Set<ClipGroupRecord>();
        public DbSet<ClipRecord> Clips => Set<ClipRecord>();


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var dbPath = Path.Combine(appDataPath, "ClipBoard", "ClipBoard.db");

            Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);
            optionsBuilder.UseSqlite($"Data Source={dbPath}");
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ClipGroupRecord>(g =>
            {
                g.ToTable("ClipGroups");
                g.HasKey(g => g.Id);

                g.Property(g => g.Name)
                    .IsRequired()
                    .HasMaxLength(200);

                g.HasMany(g => g.Clips)
                    .WithOne(c => c.ClipGroup)
                    .HasForeignKey(c => c.ClipGroupId)
                    .OnDelete(DeleteBehavior.Cascade); // When a group is deleted, delete its clips
            });
            
            modelBuilder.Entity<ClipRecord>(c =>
            {
                c.ToTable("Clips");
                c.HasKey(c => c.Id);

                c.Property(c => c.Value)
                    .IsRequired()
                    .HasColumnName("Value");

                c.Property(c => c.SortOrder)
                    .IsRequired();
            });
        }
    }
}
