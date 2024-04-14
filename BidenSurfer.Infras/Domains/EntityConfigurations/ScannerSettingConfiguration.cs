using BidenSurfer.Infras.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Newtonsoft.Json;

namespace S5E.ABPCMS.Infrastructure.EntityConfigurations
{
    public class ScannerSettingConfiguration : IEntityTypeConfiguration<ScannerSetting>
    {
        public void Configure(EntityTypeBuilder<ScannerSetting> builder)
        {
            builder.ToTable("scannersetting");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                 .HasColumnName("id")
                 .HasColumnType("BIGSERIAL")
                 .IsRequired();

            builder.Property(x => x.Userid)
                 .HasColumnName("userid")
                 .HasColumnType("bigint")
                 .IsRequired();           

            builder.Property(x => x.MaxOpen)
                 .HasColumnName("maxopen")
                 .HasColumnType("integer");            

            builder.Property(x => x.BlackList)
                 .HasColumnName("blacklist")
                 .HasColumnType("jsonb")
                 .HasConversion(
                    v => JsonConvert.SerializeObject(v),
                    v => JsonConvert.DeserializeObject<List<string>>(v));
            
        }
    }
}
