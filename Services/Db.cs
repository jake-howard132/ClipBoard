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

        public Db(DbContextOptions<Db> options): base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ClipGroupRecord>(g =>
            {
                g.ToTable("ClipGroups");
                g.HasKey(g => g.Id);

                g.Property(c => c.Id)
                    .HasDefaultValueSql("lower(hex(randomblob(4))) || '-' || lower(hex(randomblob(2))) || '-4' || substr(lower(hex(randomblob(2))),2) || '-' || substr('89ab',abs(random()) % 4 + 1,1) || substr(lower(hex(randomblob(2))),2) || '-' || lower(hex(randomblob(6)))")
                    .ValueGeneratedOnAdd();

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
                c.HasIndex(c => new { c.ClipGroupId, c.SortOrder });


                c.Property(c => c.Value)
                    .IsRequired()
                    .HasColumnName("Value");

                c.Property(c => c.SortOrder)
                    .IsRequired();
            });
        }
    }
}
