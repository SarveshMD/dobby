using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dobby.Models.Configuration;

public class UrlItemConfiguration : IEntityTypeConfiguration<UrlItem>
{
    public void Configure(EntityTypeBuilder<UrlItem> builder)
    {
        builder.ToTable("url_items");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .UseIdentityByDefaultColumn();

        builder.Property(x => x.OriginalUrl)
            .IsRequired()
            .HasMaxLength(2048);

        builder.Property(x => x.CreatedAt)
            .IsRequired();
    }
}
