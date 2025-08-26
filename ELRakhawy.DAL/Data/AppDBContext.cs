using ELRakhawy.EL.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elrakhawy.DAL.Data
{
    public class AppDBContext : DbContext
    {
        public AppDBContext(DbContextOptions<AppDBContext> options) : base(options)
        {
        }

        public DbSet<Manufacturers> Manufacturers { get; set; }
        public DbSet<PackagingStyles> PackagingStyles { get; set; }
        public DbSet<FormStyle> FormStyles { get; set; }
        public DbSet<PackagingStyleForms> PackagingStyleForms { get; set; }

        public DbSet<FinancialTransactionType> FinancialTransactionTypes { get; set; }

        public DbSet<StakeholderType> StakeholderTypes { get; set; }
        public DbSet<StakeholderTypeForm> StakeholderTypeForms { get; set; }

        public DbSet<StakeholdersInfo> StakeholderInfos { get; set; }

        public DbSet<YarnItem> YarnItems { get; set; }
        public DbSet<YarnTransaction> YarnTransactions { get; set; }
        //public DbSet<OriginYarn> OriginYarns { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            modelBuilder.Entity<YarnItem>(entity =>
            {
                entity.HasOne(y => y.OriginYarn)
                    .WithMany(y => y.DerivedYarns)
                    .HasForeignKey(y => y.OriginYarnId)
                    .OnDelete(DeleteBehavior.NoAction) // 👈 Important
                    .HasConstraintName("FK_YarnItems_OriginYarn");
            });


            // PackagingStyleForms many-to-many
            modelBuilder.Entity<PackagingStyleForms>()
                .HasKey(psf => new { psf.PackagingStyleId, psf.FormId });

            modelBuilder.Entity<PackagingStyleForms>()
                .HasOne(psf => psf.PackagingStyle)
                .WithMany(ps => ps.PackagingStyleForms)
                .HasForeignKey(psf => psf.PackagingStyleId);

            modelBuilder.Entity<PackagingStyleForms>()
                .HasOne(psf => psf.Form)
                .WithMany(f => f.PackagingStyleForms)
                .HasForeignKey(psf => psf.FormId);

            // FinancialTransactionType has data
            modelBuilder.Entity<FinancialTransactionType>().HasData(
                 new FinancialTransactionType
                 {
                     Id = 1,
                     Type = "دائن",
                     Comment = "في حالة الشراء"
                 },
                new FinancialTransactionType
                {
                    Id = 2,
                    Type = "مدين",
                    Comment = "في حالة البيع"
                }
                );
            base.OnModelCreating(modelBuilder);
            
        }
    }
    
}
