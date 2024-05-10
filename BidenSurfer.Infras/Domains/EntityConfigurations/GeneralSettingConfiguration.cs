using BidenSurfer.Infras.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Newtonsoft.Json;

namespace S5E.ABPCMS.Infrastructure.EntityConfigurations
{
    public class GeneralSettingConfiguration : IEntityTypeConfiguration<GeneralSetting>
    {
        public void Configure(EntityTypeBuilder<GeneralSetting> builder)
        {
            builder.ToTable("generalsetting");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                 .HasColumnName("id")
                 .HasColumnType("BIGSERIAL")
                 .IsRequired();

            builder.Property(x => x.Userid)
                 .HasColumnName("userid")
                 .HasColumnType("bigint")
                 .IsRequired();           

            builder.Property(x => x.Budget)
                 .HasColumnName("budget")
                 .HasColumnType("numeric(8,2)");            

            builder.Property(x => x.AssetTracking)
                 .HasColumnName("assettracking")
                 .HasColumnType("numeric(8,2)");
            
        }
    }
}
