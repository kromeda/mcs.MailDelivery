using MailDelivery.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MailDelivery.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Distribution> Distributions { get; set; }

        public DbSet<Letter> Letters { get; set; }

        public DbSet<Template> Templates { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        }
    }

    public class DistributionEntityTypeConfiguration : IEntityTypeConfiguration<Distribution>
    {
        public void Configure(EntityTypeBuilder<Distribution> builder)
        {
            builder.ToTable("Distribution");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.FromAddress).HasColumnName("SenderAddress").IsRequired();
            builder.Property(x => x.FromAlias).HasColumnName("SenderAlias").IsRequired();
            builder.Property(x => x.Status).HasConversion(typeof(string));

            builder.HasOne(x => x.Template)
                .WithMany()
                .HasForeignKey(x => x.TemplateId)
                .IsRequired();
        }
    }

    public class LetterEntityTypeConfiguration : IEntityTypeConfiguration<Letter>
    {
        public void Configure(EntityTypeBuilder<Letter> builder)
        {
            builder.ToTable("Letters");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Arguments).HasColumnName("BodyArguments");
            builder.Property(x => x.Status).HasConversion(typeof(string));

            builder
                .HasOne(x => x.Distribution)
                .WithMany(x => x.Letters)
                .HasForeignKey(x => x.DistributionId)
                .HasPrincipalKey(x => x.Id)
                .IsRequired();
        }
    }

    public class TemplateEntityTypeConfiguration : IEntityTypeConfiguration<Template>
    {
        public void Configure(EntityTypeBuilder<Template> builder)
        {
            builder.ToTable("Templates");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.BodyIsHtml).HasColumnName("IsHtml");
            builder.Property(x => x.MessageType).HasConversion(typeof(string));
            builder.Property(x => x.BodyIsHtml).HasConversion(typeof(int));
        }
    }
}