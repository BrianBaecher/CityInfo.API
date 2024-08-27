using CityInfo.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace CityInfo.API.DbContexts
{
	public class CityInfoContext : DbContext
	{
		public DbSet<City> Cities { get; set; } = null!; //Entity Framework Core will initialize this property when it sets up the DbContext, so the null! initialization is a way to satisfy the compiler while acknowledging that the framework will handle the actual initialization.

		public DbSet<PointOfInterest> PointsOfInterest { get; set; }

		//protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		//{
		//	optionsBuilder.UseSqlite("connectionstring");

		//	base.OnConfiguring(optionsBuilder);
		//}

		public CityInfoContext(DbContextOptions dbContextOptions) : base(dbContextOptions) { }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<City>().HasData(
				new City("New York City")
				{
					Id = 1,
					Description = "New York Description",
				},
				new City("Boston")
				{
					Id = 2,
					Description = "Boston description",
				},
				new City("Philadelphia")
				{
					Id = 3,
					Description = "Philly description",
				});

			modelBuilder.Entity<PointOfInterest>().HasData(
				new PointOfInterest("Times Square")
				{
					Id = 1,
					CityId = 1,
					Description = "Tourist trap."
				},
				new PointOfInterest("Central Park")
				{
					Id = 2,
					CityId = 1,
					Description = "Big park",
				},
				new PointOfInterest("Boston Harbor")
				{
					Id = 3,
					CityId = 2,
					Description = "Tea in the water",
				},
				new PointOfInterest("Boston Aquarium")
				{
					Id = 4,
					CityId = 2,
					Description = "Fish and stuff",
				},
				new PointOfInterest("Kensington Ave")
				{
					Id = 5,
					CityId = 3,
					Description = "Drug market"
				},
				new PointOfInterest("Love Park")
				{
					Id = 6,
					CityId = 3,
					Description = "Designed by Kevin Bacon's dad"
				});

			base.OnModelCreating(modelBuilder);
		}

	}
}
