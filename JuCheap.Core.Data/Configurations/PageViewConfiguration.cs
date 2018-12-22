﻿using JuCheap.Core.Data.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JuCheap.Core.Data.Configurations
{
    /// <summary>
    /// PageView表信息配置
    /// </summary>
    public class PageViewConfiguration : BaseConfiguration<PageViewEntity>
    {
        public override void Configure(EntityTypeBuilder<PageViewEntity> builder)
        {
            base.Configure(builder);

            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id).HasMaxLength(36).ValueGeneratedNever();
            builder.Property(e => e.LoginName).HasMaxLength(20).IsRequired();
            builder.Property(e => e.IP).HasMaxLength(20).IsRequired();
            builder.Property(e => e.Url).HasMaxLength(300).IsRequired();
            builder.Property(e => e.UserId).HasMaxLength(36).IsRequired();
            builder.Property(e => e.CreateDateTime).IsRequired();
            builder.Property(e => e.IsDeleted).IsRequired();
            builder.ToTable("PageViews");
        }
    }
}
