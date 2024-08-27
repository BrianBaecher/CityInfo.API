using CityInfo.API.DbContexts;
using CityInfo.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace CityInfo.API.Services
{
	public class CityInfoRepository : ICityInfoRepository
	{
		private readonly CityInfoContext _cityInfoContext;


		public CityInfoRepository(CityInfoContext cityInfoContext)
		{
			_cityInfoContext = cityInfoContext ?? throw new ArgumentNullException(nameof(cityInfoContext));
		}


		public async Task<IEnumerable<City>> GetCitiesAsync()
		{
			return await _cityInfoContext.Cities.OrderBy(c => c.Name).ToListAsync();
		}


		public async Task<City?> GetCityAsync(int cityId, bool includePointsOfInterest)
		{

			if (includePointsOfInterest)
			{
				return await _cityInfoContext.Cities.Include(c => c.PointsOfInterest).Where(c => c.Id == cityId).FirstOrDefaultAsync();
			}
			return await _cityInfoContext.Cities.Where(c => c.Id == cityId).FirstOrDefaultAsync();

		}


		public async Task<PointOfInterest?> GetPointOfInterestAsync(int cityId, int pointOfInterestId)
		{
			return await _cityInfoContext.PointsOfInterest.Where(p => p.Id == pointOfInterestId && p.CityId == cityId).FirstOrDefaultAsync();

		}


		public async Task<IEnumerable<PointOfInterest>> GetPointsOfInterestForCityAsync(int cityId)
		{
			return await _cityInfoContext.PointsOfInterest.Where(p => p.CityId == cityId).ToListAsync();
		}


		public async Task<bool> CityExistsAsync(int cityId)
		{
			return await _cityInfoContext.Cities.AnyAsync(c => c.Id == cityId);
		}

		public async Task AddPointOfInterestForCity(int cityId, PointOfInterest pointOfInterest)
		{
			var city = await GetCityAsync(cityId, false);

			if (city != null)
			{
				city.PointsOfInterest.Add(pointOfInterest);
			}
		}

		public async Task<bool> SaveChangesAsync()
		{
			return await _cityInfoContext.SaveChangesAsync() >= 0; //DbContext.SaveChangesAsync() returns the number of changes occurring
		}

		public void DeletePointOfInterest(PointOfInterest pointOfInterest)
		{
			_cityInfoContext.PointsOfInterest.Remove(pointOfInterest);
		}
	}
}
