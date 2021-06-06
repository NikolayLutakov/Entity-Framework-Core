namespace VaporStore.Data
{
	using Microsoft.EntityFrameworkCore;
    using VaporStore.Data.Models;

    public class VaporStoreDbContext : DbContext
	{
		public VaporStoreDbContext()
		{
		}

		public VaporStoreDbContext(DbContextOptions options)
			: base(options)
		{
		}

        public virtual DbSet<Card> Cards { get; set; }
		public virtual DbSet<Developer> Developers { get; set; }
		public virtual DbSet<Game> Games { get; set; }
		public virtual DbSet<GameTag> GameTags { get; set; }
		public virtual DbSet<Genre> Genres { get; set; }
		public virtual DbSet<Purchase> Purchases { get; set; }
		public virtual DbSet<Tag> Tags { get; set; }
		public virtual DbSet<User> Users { get; set; }

		protected override void OnConfiguring(DbContextOptionsBuilder options)
		{
			if (!options.IsConfigured)
			{
				options
					.UseSqlServer(Configuration.ConnectionString);
			}
		}

		protected override void OnModelCreating(ModelBuilder model)
		{
			//model.Entity<Game>(game =>
			//{
				//game.HasCheckConstraint("CK_Price", "Price >= 0");

				//game.HasKey(x => x.Id);

				//game.Property(x => x.Name).IsRequired();

				//game.Property(x => x.GameTags).IsRequired(true);
			//});

			model.Entity<GameTag>(gt =>
			{
				gt.HasKey(x => new { x.GameId, x.TagId });

				gt.HasOne(x => x.Game)
				.WithMany(x => x.GameTags)
				.HasForeignKey(x => x.GameId)
				.OnDelete(DeleteBehavior.Restrict);

				gt.HasOne(x => x.Tag)
				.WithMany(x => x.GameTags)
				.HasForeignKey(x => x.TagId)
				.OnDelete(DeleteBehavior.Restrict);
			});

			model.Entity<Purchase>(purchase => 
			{
				purchase.HasKey(x => x.Id);

				purchase.HasOne(x => x.Game)
				.WithMany(x => x.Purchases)
				.HasForeignKey(x => x.GameId)
				.OnDelete(DeleteBehavior.Restrict);

				purchase.HasOne(x => x.Card)
				.WithMany(x => x.Purchases)
				.HasForeignKey(x => x.CardId)
				.OnDelete(DeleteBehavior.Restrict);
			});
		}
	}
}