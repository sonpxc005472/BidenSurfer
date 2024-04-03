using BidenSurfer.Infras.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace S5E.ABPCMS.Infrastructure.EntityConfigurations
{
    public class UserSettingConfiguration : IEntityTypeConfiguration<UserSetting>
    {
        public void Configure(EntityTypeBuilder<UserSetting> builder)
        {
            builder.ToTable("usersettings");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                 .HasColumnName("id")
                 .HasColumnType("BIGSERIAL")
                 .IsRequired();
            builder.Property(x => x.UserId)
                 .HasColumnName("userid")
                 .HasColumnType("Bigint")
                 .IsRequired();
            builder.Property(x => x.ApiKey)
                 .HasColumnName("apikey")
                 .HasColumnType("varchar")
                 .HasMaxLength(50);
            builder.Property(x => x.SecretKey)
                 .HasColumnName("secretkey")
                 .HasColumnType("varchar")
                 .HasMaxLength(50);
            builder.Property(x => x.PassPhrase)
                 .HasColumnName("passphrase")
                 .HasColumnType("varchar")
                 .HasMaxLength(50);
            builder.Property(x => x.TeleChannel)
                 .HasColumnName("telechannel")
                 .HasColumnType("varchar")
                 .HasMaxLength(20);
            builder.HasOne(x => x.User)
                .WithOne(x => x.UserSetting);
        }
    }
}
