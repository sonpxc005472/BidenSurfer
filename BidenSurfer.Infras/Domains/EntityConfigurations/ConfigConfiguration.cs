using BidenSurfer.Infras.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace S5E.ABPCMS.Infrastructure.EntityConfigurations
{
    public class ConfigConfiguration : IEntityTypeConfiguration<Config>
    {
        public void Configure(EntityTypeBuilder<Config> builder)
        {
            builder.ToTable("configs");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                 .HasColumnName("id")
                 .HasColumnType("BIGSERIAL")
                 .IsRequired();

            builder.Property(x => x.Userid)
                 .HasColumnName("userid")
                 .HasColumnType("bigint")
                 .IsRequired();

            builder.Property(x => x.Symbol)
                 .HasColumnName("symbol")
                 .HasColumnType("varchar")
                 .HasMaxLength(50);

            builder.Property(x => x.CustomId)
                 .HasColumnName("customid")
                 .HasColumnType("varchar")
                 .HasMaxLength(100);

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
            

            builder.Property(x => x.IncreaseOcPercent)
                 .HasColumnName("increaseocpercent")
                 .HasColumnType("integer");

            builder.Property(x => x.Amount)
                 .HasColumnName("amount")
                 .HasColumnType("numeric(8,2)");

            builder.Property(x => x.OriginAmount)
                 .HasColumnName("originamount")
                 .HasColumnType("numeric(8,2)");

            builder.Property(x => x.IncreaseAmountPercent)
                 .HasColumnName("increaseamountpercent")
                 .HasColumnType("integer");

            builder.Property(x => x.AmountLimit)
                 .HasColumnName("amountlimit")
                 .HasColumnType("numeric(8,2)");

            builder.Property(x => x.IncreaseAmountExpire)
                 .HasColumnName("amountexpire")
                 .HasColumnType("integer");

            builder.Property(x => x.Expire)
                 .HasColumnName("expire")
                 .HasColumnType("integer");

            builder.Property(x => x.CreatedBy)
                 .HasColumnName("createdby")
                 .HasColumnType("varchar")
                 .HasMaxLength(200);

            builder.Property(x => x.CreatedDate)
                 .HasColumnName("createddate")
                 .HasColumnType("timestamp");

            builder.Property(x => x.EditedDate)
                 .HasColumnName("editeddate")
                 .HasColumnType("timestamp");

            builder.Property(x => x.IsActive)
                 .HasColumnName("isactive")
                 .HasColumnType("boolean");
            

            builder.HasOne(x => x.User)
                .WithMany(x => x.Configs)
                .HasForeignKey(x => x.Userid);
        }
    }
}
