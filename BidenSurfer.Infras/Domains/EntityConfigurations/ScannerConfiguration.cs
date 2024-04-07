using BidenSurfer.Infras.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Newtonsoft.Json;

namespace S5E.ABPCMS.Infrastructure.EntityConfigurations
{
    public class ScannerConfiguration : IEntityTypeConfiguration<Scanner>
    {
        public void Configure(EntityTypeBuilder<Scanner> builder)
        {
            builder.ToTable("scanner");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                 .HasColumnName("id")
                 .HasColumnType("BIGSERIAL")
                 .IsRequired();

            builder.Property(x => x.Userid)
                 .HasColumnName("userid")
                 .HasColumnType("bigint")
                 .IsRequired();

            builder.Property(x => x.Title)
                 .HasColumnName("title")
                 .HasColumnType("varchar")
                 .HasMaxLength(200);

            builder.Property(x => x.PositionSide)
                 .HasColumnName("positionside")
                 .HasColumnType("varchar")
                 .HasMaxLength(20);           

            builder.Property(x => x.OrderChange)
                 .HasColumnName("orderchange")
                 .HasColumnType("numeric(4,2)");
            
            builder.Property(x => x.OrderType)
                 .HasColumnName("ordertype")
                 .HasColumnType("integer");          
            

            builder.Property(x => x.OcNumber)
                 .HasColumnName("ocnumber")
                 .HasColumnType("integer");

            builder.Property(x => x.Amount)
                 .HasColumnName("amount")
                 .HasColumnType("numeric(8,2)");

            builder.Property(x => x.AmountExpire)
                 .HasColumnName("amountexpire")
                 .HasColumnType("integer");

            builder.Property(x => x.AutoAmount)
                 .HasColumnName("autoamount")
                 .HasColumnType("integer");

            builder.Property(x => x.AmountLimit)
                 .HasColumnName("amountlimit")
                 .HasColumnType("numeric(8,2)");

            builder.Property(x => x.Turnover)
                .HasColumnName("turnover")
                .HasColumnType("numeric(8,2)");

            builder.Property(x => x.Elastic)
                .HasColumnName("elastic")
                .HasColumnType("integer");

            builder.Property(x => x.AmountLimit)
                .HasColumnName("amountlimit")
                .HasColumnType("numeric(8,2)");

            builder.Property(x => x.ConfigExpire)
                .HasColumnName("configexpire")
                .HasColumnType("integer");

            builder.Property(x => x.OnlyPairs)
                 .HasColumnName("onlypairs")
                 .HasColumnType("jsonb")
                 .HasConversion(
                    v => JsonConvert.SerializeObject(v),
                    v => JsonConvert.DeserializeObject<List<string>>(v));

            builder.Property(x => x.BlackList)
                 .HasColumnName("blacklist")
                 .HasColumnType("jsonb")
                 .HasConversion(
                    v => JsonConvert.SerializeObject(v),
                    v => JsonConvert.DeserializeObject<List<string>>(v));

            builder.Property(x => x.IsActive)
                 .HasColumnName("isactive")
                 .HasColumnType("boolean");
            

            //builder.HasOne(x => x.User)
            //    .WithMany(x => x.Scanners)
            //    .HasForeignKey(x => x.Userid);
        }
    }
}
