﻿using JuCheap.Core.Data.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JuCheap.Core.Data.Configurations
{
    /// <summary>
    /// User表信息配置
    /// </summary>
    public class UserConfiguration : BaseConfiguration<UserEntity>
    {
        public override void Configure(EntityTypeBuilder<UserEntity> builder)
        {
            base.Configure(builder);

            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id).HasMaxLength(36).ValueGeneratedNever();
            builder.Property(e => e.LoginName).HasMaxLength(20).IsRequired();
            builder.Property(e => e.RealName).HasMaxLength(20).IsRequired();
            builder.Property(e => e.Email).HasMaxLength(36).IsRequired();
            builder.Property(e => e.Password).HasMaxLength(50).IsRequired();
            builder.Property(e => e.IsSuperMan).IsRequired();
            builder.Property(e => e.CreateDateTime).IsRequired();
            builder.Property(e => e.IsDeleted).IsRequired();
            builder.Property(e => e.DepartmentId).HasMaxLength(36);
            builder.HasOne(e => e.Department).WithMany(e => e.Users).HasForeignKey(e => e.DepartmentId);
            builder.ToTable("Users");
        }
    }
}
