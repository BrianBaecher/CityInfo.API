using CityInfo.API.Models;
using Microsoft.AspNetCore.Mvc;

namespace CityInfo.API.Controllers
{
	[Route("api/cities/{cityId}/pointsofinterest")]
	[ApiController]
	public class PointsOfInterestController : ControllerBase
	{
		[HttpGet]
		public ActionResult<IEnumerable<PointOfInterestDto>> GetPointsOfInterest(int cityId)
		{
			var city = CitiesDataStore.Current.Cities.FirstOrDefault(c => c.Id == cityId);

			if (city == null)
			{
				return NotFound();
			}

			return Ok(city.PointsOfInterest);
		}

		[HttpGet("{pointOfInterestId}", Name = "GetPointOfInterest")]
		public ActionResult<PointOfInterestDto> GetPointOfInterest(int cityId, int pointOfInterestId)
		{
			var city = CitiesDataStore.Current.Cities.FirstOrDefault(c => c.Id == cityId);
			if (city == null)
			{
				return NotFound();
			}

			var poi = city.PointsOfInterest.FirstOrDefault(p => p.Id == pointOfInterestId);
			if (poi == null)
			{
				return NotFound();
			}
			return Ok(poi);
		}

		[HttpPost]
		public ActionResult<PointOfInterestDto> CreatePointOfInterest(
			int cityId,
			PointOfInterestForCreationDto pointOfInterestForCreationDto // is coming [FromBody], without being specified
			)
		{
			// check for valid city
			var city = CitiesDataStore.Current.Cities.FirstOrDefault(c => cityId == c.Id);
			if (city == null)
			{
				return NotFound();
			}

			// demo - assigning ID - will need to be improved...
			var highPoiId = CitiesDataStore.Current.Cities.SelectMany(c => c.PointsOfInterest).Max(p => p.Id);

			// create new POI object (not the creation one)
			var poiToAdd = new PointOfInterestDto()
			{
				Id = highPoiId + 1,
				Name = pointOfInterestForCreationDto.Name,
				Description = pointOfInterestForCreationDto.Description,
			};

			// add to list of poi
			city.PointsOfInterest.Add(poiToAdd);

			return CreatedAtRoute("GetPointOfInterest", new
			{
				cityId = cityId,
				pointOfInterestId = poiToAdd.Id
			},
			poiToAdd);
		}

		[HttpPut("{pointOfInterestId}")]
		public ActionResult<PointOfInterestDto> UpdatePointOfInterest(int cityId, int pointOfInterestId, PointOfInterestForUpdateDto forUpdateDto)
		{
			// get city
			var city = CitiesDataStore.Current.Cities.FirstOrDefault(c => c.Id == cityId);

			if (city == null)
			{
				return NotFound();
			}

			// get poi
			var poi = city.PointsOfInterest.FirstOrDefault(p => p.Id == pointOfInterestId);

			if (poi == null)
			{
				return NotFound();
			}

			// update properties
			poi.Name = forUpdateDto.Name;
			poi.Description = forUpdateDto.Description;

			return NoContent(); // could also return the POI, but not necessary as it's coming from client anyway...
		}





	}
}
