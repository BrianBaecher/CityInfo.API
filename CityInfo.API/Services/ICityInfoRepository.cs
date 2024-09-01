using CityInfo.API.Entities;

namespace CityInfo.API.Services
{
	public interface ICityInfoRepository
	{
		Task<IEnumerable<City>> GetCitiesAsync();

		Task<(IEnumerable<City>, PaginationMetadata)> GetCitiesAsync(string? name, string? searchQuery, int pageNumber, int pageSize);

		Task<City?> GetCityAsync(int cityId, bool includePointsOfInterest);

		Task<PointOfInterest?> GetPointOfInterestAsync(int cityId, int pointOfInterestId);

		Task<bool> CityExistsAsync(int cityId);

		Task<IEnumerable<PointOfInterest>> GetPointsOfInterestForCityAsync(int cityId);

		Task AddPointOfInterestForCity(int cityId, PointOfInterest pointOfInterest);

		Task<bool> SaveChangesAsync();

		void DeletePointOfInterest(PointOfInterest pointOfInterest);
	}
}
