using CityInfo.API.Models;

namespace CityInfo.API
{
	public class CitiesDataStore
	{
		public List<CityDto> Cities { get; set; }

		//public static CitiesDataStore Current { get; } = new CitiesDataStore();

		public CitiesDataStore()
		{
			Cities = new List<CityDto>()
			{
				new CityDto()
				{
					Id = 1,
					Name = "New York",
					Description = "NY description",
					PointsOfInterest = new List<PointOfInterestDto>()
					{
						new PointOfInterestDto()
						{
							Id = 1,
							Name = "Bagel store",
							Description = "A store to buy bagels from."
						},
						new PointOfInterestDto()
						{
							Id = 2,
							Name = "Hunter College",
							Description = "A college I went to that one time."
						}
					}
				},
				new CityDto()
				{
					Id= 2,
					Name = "Boston",
					Description = "Boston description",
					PointsOfInterest = new List<PointOfInterestDto>()
					{
						new PointOfInterestDto()
						{
							Id = 3,
							Name = "Paul Revere's House",
							Description = "That guy who was yelling about that thing."
						},
						new PointOfInterestDto()
						{
							Id = 4,
							Name = "Waterfront",
							Description = "Boats and stuff."
						}
					}
				},
				new CityDto()
				{
					Id = 3,
					Name = "Philadelphia",
					Description = "Philly description",
					PointsOfInterest = new List<PointOfInterestDto>()
					{
						new PointOfInterestDto()
						{
							Id = 5,
							Name = "Liberty Bell",
							Description = "That bell that still isn't fixed for some reason."
						},
						new PointOfInterestDto()
						{
							Id = 6,
							Name = "City Hall",
							Description = "Where the government does stuff."
						}
					}
				}
			};
		}
	}
}
