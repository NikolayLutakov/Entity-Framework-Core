namespace SoftJail.Data
{
	using Microsoft.EntityFrameworkCore;
    using SoftJail.Data.Models;

    public class SoftJailDbContext : DbContext
	{
		public SoftJailDbContext()
		{
		}

		public SoftJailDbContext(DbContextOptions options)
			: base(options)
		{
		}


        public DbSet<Cell> Cells { get; set; }

        public DbSet<Department> Departments { get; set; }

        public DbSet<Mail> Mails { get; set; }

        public DbSet<Officer> Officers { get; set; }

        public DbSet<OfficerPrisoner> OfficersPrisoners { get; set; }

        public DbSet<Prisoner> Prisoners { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			if (!optionsBuilder.IsConfigured)
			{
				optionsBuilder
					.UseSqlServer(Configuration.ConnectionString);
			}
		}

		protected override void OnModelCreating(ModelBuilder builder)
		{
			builder.Entity<OfficerPrisoner>(officerPrisoner =>
			{
				officerPrisoner.HasKey(x => new { x.PrisonerId, x.OfficerId });

				officerPrisoner.HasOne(op => op.Prisoner)
				.WithMany(p => p.PrisonerOfficers)
				.HasForeignKey(op => op.PrisonerId)
				.OnDelete(DeleteBehavior.Restrict);

				officerPrisoner.HasOne(op => op.Officer)
				.WithMany(p => p.OfficerPrisoners)
				.HasForeignKey(op => op.OfficerId)
				.OnDelete(DeleteBehavior.Restrict);
			});
		}
	}
}