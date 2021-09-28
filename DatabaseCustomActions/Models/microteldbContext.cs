using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

#nullable disable

namespace DatabaseCustomActions.Models
{
    public partial class microteldbContext : DbContext
    {
        public microteldbContext()
        {
        }

        public microteldbContext(DbContextOptions<microteldbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Bill> Bills { get; set; }
        public virtual DbSet<ExtraPackage> ExtraPackages { get; set; }
        public virtual DbSet<ExtraPackageDetail> ExtraPackageDetails { get; set; }
        public virtual DbSet<Line> Lines { get; set; }
        public virtual DbSet<Payment> Payments { get; set; }
        public virtual DbSet<Quotum> Quota { get; set; }
        public virtual DbSet<TierDetail> TierDetails { get; set; }
        public virtual DbSet<User> Users { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                string connectionString = Environment.GetEnvironmentVariable("ContosoTel-connectionString");
                optionsBuilder.UseSqlServer(connectionString);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("Relational:Collation", "SQL_Latin1_General_CP1_CI_AS");

            modelBuilder.Entity<Bill>(entity =>
            {
                entity.ToTable("bill");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasDefaultValueSql("(newid())");

                entity.Property(e => e.Amount)
                    .HasColumnType("money")
                    .HasColumnName("amount");

                entity.Property(e => e.DueDate)
                    .HasColumnType("date")
                    .HasColumnName("dueDate");

                entity.Property(e => e.IsPaid).HasColumnName("isPaid");

                entity.Property(e => e.PhoneNumber)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("phoneNumber");

                entity.Property(e => e.TeirId).HasColumnName("teirID");

                entity.HasOne(d => d.PhoneNumberNavigation)
                    .WithMany(p => p.Bills)
                    .HasForeignKey(d => d.PhoneNumber)
                    .HasConstraintName("FK__bill__phoneNumbe__09746778");

                entity.HasOne(d => d.Teir)
                    .WithMany(p => p.Bills)
                    .HasForeignKey(d => d.TeirId)
                    .HasConstraintName("FK__bill__teirID__0B5CAFEA");
            });

            modelBuilder.Entity<ExtraPackage>(entity =>
            {
                entity.ToTable("extra_package");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasDefaultValueSql("(newid())");

                entity.Property(e => e.Date)
                    .HasColumnType("datetime")
                    .HasColumnName("date");

                entity.Property(e => e.ExtraPackageId).HasColumnName("extraPackageID");

                entity.Property(e => e.PhoneNumber)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("phoneNumber");

                entity.Property(e => e.TotalPrice)
                    .HasColumnType("money")
                    .HasColumnName("totalPrice");

                entity.HasOne(d => d.ExtraPackageNavigation)
                    .WithMany(p => p.ExtraPackages)
                    .HasForeignKey(d => d.ExtraPackageId)
                    .HasConstraintName("FK__ExtraPack__extra__540C7B00");

                entity.HasOne(d => d.PhoneNumberNavigation)
                    .WithMany(p => p.ExtraPackages)
                    .HasForeignKey(d => d.PhoneNumber)
                    .HasConstraintName("FK__ExtraPack__phone__531856C7");
            });

            modelBuilder.Entity<ExtraPackageDetail>(entity =>
            {
                entity.ToTable("extra_package_details");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasDefaultValueSql("(newid())");

                entity.Property(e => e.Megabytes).HasColumnName("megabytes");

                entity.Property(e => e.Messages).HasColumnName("messages");

                entity.Property(e => e.Minutes).HasColumnName("minutes");

                entity.Property(e => e.Name)
                    .HasMaxLength(30)
                    .IsUnicode(false)
                    .HasColumnName("name");

                entity.Property(e => e.Price)
                    .HasColumnType("money")
                    .HasColumnName("price");
            });

            modelBuilder.Entity<Line>(entity =>
            {
                entity.HasKey(e => e.PhoneNumber)
                    .HasName("PK__line__4849DA00D9319AD2");

                entity.ToTable("line");

                entity.HasIndex(e => e.QuotaId, "UQ__line__DC48C72302D65AAF")
                    .IsUnique();

                entity.Property(e => e.PhoneNumber)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("phoneNumber");

                entity.Property(e => e.QuotaId).HasColumnName("quotaID");

                entity.Property(e => e.TierId).HasColumnName("tierID");

                entity.HasOne(d => d.Quota)
                    .WithOne(p => p.Line)
                    .HasForeignKey<Line>(d => d.QuotaId)
                    .HasConstraintName("FK__line__quotaID__43D61337");

                entity.HasOne(d => d.Tier)
                    .WithMany(p => p.Lines)
                    .HasForeignKey(d => d.TierId)
                    .HasConstraintName("FK__line__tierID__42E1EEFE");
            });

            modelBuilder.Entity<Payment>(entity =>
            {
                entity.ToTable("payment");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasDefaultValueSql("(newid())");

                entity.Property(e => e.Amount)
                    .HasColumnType("money")
                    .HasColumnName("amount");

                entity.Property(e => e.BillId).HasColumnName("billID");

                entity.Property(e => e.CreditCard)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("creditCard");

                entity.Property(e => e.Date)
                    .HasColumnType("datetime")
                    .HasColumnName("date");

                entity.HasOne(d => d.Bill)
                    .WithMany(p => p.Payments)
                    .HasForeignKey(d => d.BillId)
                    .HasConstraintName("FK__payment__billID__0F2D40CE");
            });

            modelBuilder.Entity<Quotum>(entity =>
            {
                entity.ToTable("quota");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasDefaultValueSql("(newid())");

                entity.Property(e => e.Date)
                    .HasColumnType("date")
                    .HasColumnName("date");

                entity.Property(e => e.RemainingMegabytes).HasColumnName("remainingMegabytes");

                entity.Property(e => e.RemainingMessages).HasColumnName("remainingMessages");

                entity.Property(e => e.RemainingMinutes).HasColumnName("remainingMinutes");
            });

            modelBuilder.Entity<TierDetail>(entity =>
            {
                entity.ToTable("tier_details");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasDefaultValueSql("(newid())");

                entity.Property(e => e.Megabytes).HasColumnName("megabytes");

                entity.Property(e => e.Messages).HasColumnName("messages");

                entity.Property(e => e.Minutes).HasColumnName("minutes");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("name");

                entity.Property(e => e.Price)
                    .HasColumnType("money")
                    .HasColumnName("price");
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.NationalId)
                    .HasName("PK__user__B5881E89094FFD8D");

                entity.ToTable("user");

                entity.Property(e => e.NationalId)
                    .ValueGeneratedNever()
                    .HasColumnName("nationalID");

                entity.Property(e => e.BirthDate)
                    .HasColumnType("date")
                    .HasColumnName("birthDate");

                entity.Property(e => e.City)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("city");

                entity.Property(e => e.Country)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("country");

                entity.Property(e => e.FName)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("fName");

                entity.Property(e => e.LName)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("lName");

                entity.Property(e => e.PhoneNumber)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("phoneNumber");

                entity.Property(e => e.StreetName)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("streetName");

                entity.Property(e => e.StreetNo).HasColumnName("streetNo");

                entity.Property(e => e.Timestamp)
                    .HasColumnType("datetime")
                    .HasColumnName("timestamp")
                    .HasDefaultValueSql("(getdate())");

                entity.HasOne(d => d.PhoneNumberNavigation)
                    .WithMany(p => p.Users)
                    .HasForeignKey(d => d.PhoneNumber)
                    .HasConstraintName("FK__user__phoneNumbe__6EC0713C");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
