using CityInfo.API.DbContexts;
using CityInfo.API.Entities;
using Microsoft.AspNetCore.Mvc;
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

		public async Task<(IEnumerable<City>, PaginationMetadata)> GetCitiesAsync(
			[FromQuery] string? name,
			[FromQuery] string? searchQuery,
			[FromQuery] int pageNumber,
			[FromQuery] int pageSize
			)
		{
			// handle searching or filtering, or both. depending on whether strings are present...
			// cast DbSet of Cities to a queryable 
			var collection = _cityInfoContext.Cities as IQueryable<City>;

			// check filter first (name is a filter...)
			if (!string.IsNullOrWhiteSpace(name))
			{
				name = name.Trim();
				collection = collection.Where(c => c.Name == name);
			}

			if (!string.IsNullOrWhiteSpace(searchQuery))
			{
				searchQuery = searchQuery.Trim();
				collection = collection.Where(c => c.Name.ToLower().Contains(searchQuery.ToLower())
				|| c.Description != null && c.Description.ToLower().Contains(searchQuery.ToLower())
				);
			}

			var totalItemCount = await collection.CountAsync();

			var paginationMetadata = new PaginationMetadata(
				totalItemCount,
				pageSize,
				pageNumber
				);

			// calling ToListAsync marks the end of the deferred execution of query, so this is when the database actually receives an SQL statement.
			// calling Skip() applies the pagination, important to do this last, as you don't want to skip over the potentially matching entries...
			var collectionToReturn = await collection
				.OrderBy(c => c.Name)
				.Skip(pageSize * (pageNumber - 1))
				.Take(pageSize)
				.ToListAsync();

			return (collectionToReturn, paginationMetadata);
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
