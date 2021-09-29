using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace DatabaseCustomActions.Models
{
    public static class ModelBuilderExtension
    {
        public static void Seed(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TierDetail>().HasData(GetTiers());
            modelBuilder.Entity<ExtraPackageDetail>().HasData(GetExtraPackages());
        }
        public static List<TierDetail> GetTiers()
        {
            List<TierDetail> tiers = new List<TierDetail> {
                new TierDetail {Id=Guid.NewGuid(),Name = "Standard", Minutes =1000,Messages=500,Megabytes= 10000,Price=10},
                new TierDetail {Id=Guid.NewGuid(),Name = "Premium", Minutes =5000,Messages=2500,Megabytes= 50000,Price=30},
                new TierDetail {Id=Guid.NewGuid(),Name = "VIP", Minutes =10000,Messages=5000,Megabytes= 100000,Price=50},
            };
            return tiers;
        }
        public static List<ExtraPackageDetail> GetExtraPackages()
        {
            List<ExtraPackageDetail> tiers = new List<ExtraPackageDetail> {
                new ExtraPackageDetail {Id=Guid.NewGuid(), Name = "Plus Package", Minutes =2000,Messages=200,Megabytes= 2000,Price=5},
                new ExtraPackageDetail {Id=Guid.NewGuid(),Name = "Premium Plus Package", Minutes =3000,Messages=300,Megabytes= 5000,Price=15},

                new ExtraPackageDetail {Id=Guid.NewGuid(),Name = "Minutes 5 Plus", Minutes =1000,Messages=0,Megabytes= 0,Price=5},
                new ExtraPackageDetail {Id=Guid.NewGuid(),Name = "Minutes 15 Plus", Minutes =3000,Messages=0,Megabytes= 0,Price=15},
                new ExtraPackageDetail {Id=Guid.NewGuid(),Name = "Minutes 25 Plus", Minutes =5000,Messages=0,Megabytes= 0,Price=25},

                new ExtraPackageDetail {Id=Guid.NewGuid(),Name = "Text Messages 1 Plus", Minutes =0,Messages=100,Megabytes= 0,Price=1},
                new ExtraPackageDetail {Id=Guid.NewGuid(),Name = "Text Messages 5 Plus", Minutes =0,Messages=500,Megabytes= 0,Price=5},
                new ExtraPackageDetail {Id=Guid.NewGuid(),Name = "Text Messages 10 Plus", Minutes =0,Messages=1000,Megabytes= 0,Price=10},

                new ExtraPackageDetail {Id=Guid.NewGuid(),Name = "Megabytes 1 Plus", Minutes =0,Messages=0,Megabytes= 1000,Price=1},
                new ExtraPackageDetail {Id=Guid.NewGuid(),Name = "Megabytes 5 Plus", Minutes =0,Messages=0,Megabytes= 5000,Price=5},
                new ExtraPackageDetail {Id=Guid.NewGuid(),Name = "Megabytes 10 Plus", Minutes =0,Messages=0,Megabytes= 10000,Price=10},
            };
            return tiers;
        }
    }
}
