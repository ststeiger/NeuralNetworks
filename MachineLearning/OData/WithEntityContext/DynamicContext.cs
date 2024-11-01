
namespace MachineLearning.foobar20000
{
    using System;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;


    public class Benutzer
    {
        public int BE_ID { get; set; }
        public string BE_Name { get; set; }
        public DateTimeOffset? BE_CreatedDate { get; set; }
    }


    public class BenutzerConfiguration 
        : IEntityTypeConfiguration<Benutzer>
    {
        public void Configure(EntityTypeBuilder<Benutzer> builder)
        {
            builder.ToTable("T_Benutzer");

            // builder.Metadata.SetTableName("T_Benutzer");
            // builder.Metadata.SetSchema(null);

            builder.HasKey(e => e.BE_ID);
            builder.Property(e => e.BE_Name).HasMaxLength(50);
        }
    }


    public class DynamicContext 
        : DbContext
    {

        private readonly DbContextOptions _options;


        // Add parameterless constructor
        public DynamicContext()
        { }



        public DynamicContext(DbContextOptions options)
            : base(options)
        {
            _options = options;
        }

        public DbSet<Benutzer> Benutzer { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            /*
            modelBuilder.Entity<Benutzer>(entity =>
            {
                entity.ToTable(""T_Benutzer"");
                entity.HasKey(e => e.BE_ID);
                entity.Property(e => e.BE_Name).HasMaxLength(50);
            });
            */
            modelBuilder.ApplyConfiguration(new BenutzerConfiguration());
        }
    }
}
