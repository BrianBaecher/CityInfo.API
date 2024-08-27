using CityInfo.API.Entities;

namespace CityInfo.API.Services
{
	public interface ICityInfoRepository
	{
		Task<IEnumerable<City>> GetCitiesAsync();

		Task<City?> GetCityAsync(int cityId, bool includePointsOfInterest);

		Task<PointOfInterest?> GetPointOfInterestAsync(int cityId, int pointOfInterestId);

		Task<bool> CityExistsAsync(int cityId);

		Task<IEnumerable<PointOfInterest>> GetPointsOfInterestForCityAsync(int cityId);

		Task AddPointOfInterestForCity(int cityId, PointOfInterest pointOfInterest);

		Task<bool> SaveChangesAsync();

		void DeletePointOfInterest(PointOfInterest pointOfInterest);
	}
}
