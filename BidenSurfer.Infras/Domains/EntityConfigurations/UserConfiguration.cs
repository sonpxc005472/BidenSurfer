using System;
using BidenSurfer.Infras.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace S5E.ABPCMS.Infrastructure.EntityConfigurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("users");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                 .HasColumnName("id")
                 .HasColumnType("BIGSERIAL")
                 .IsRequired();
            builder.Property(x => x.FullName)
                 .HasColumnName("fullname")
                 .HasColumnType("varchar")
                 .HasMaxLength(50)
                 .IsRequired();
            builder.Property(x => x.Username)
                 .HasColumnName("username")
                 .HasColumnType("varchar")
                 .HasMaxLength(50)
                 .IsRequired();
            builder.Property(x => x.Password)
                 .HasColumnName("password")
                 .HasColumnType("varchar")
                 .HasMaxLength(50)
                 .IsRequired();
            builder.Property(x => x.Email)
                 .HasColumnName("email")
                 .HasColumnType("varchar")
                 .HasMaxLength(50);
            builder.Property(x => x.Role)
                 .HasColumnName("role")
                 .HasColumnType("integer");
            builder.Property(x => x.Status)
                 .HasColumnName("status")
                 .HasColumnType("integer");
        }
    }
}
