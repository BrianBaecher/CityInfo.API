using CityInfo.API.Models;
using CityInfo.API.Services;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace CityInfo.API.Controllers
{
	[Route("api/cities/{cityId}/pointsofinterest")]
	[ApiController]
	public class PointsOfInterestController : ControllerBase
	{
		private readonly ILogger<PointsOfInterestController> _logger;
		private readonly IMailService _mailService;
		private readonly CitiesDataStore _citiesDataStore;

		public PointsOfInterestController(ILogger<PointsOfInterestController> logger, IMailService mailService, CitiesDataStore citiesDataStore)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_mailService = mailService ?? throw new ArgumentNullException(nameof(mailService));
			_citiesDataStore = citiesDataStore ?? throw new ArgumentNullException(nameof(citiesDataStore));
		}

		[HttpGet]
		public ActionResult<IEnumerable<PointOfInterestDto>> GetPointsOfInterest(int cityId)
		{
			try
			{
				var city = _citiesDataStore.Cities.FirstOrDefault(c => c.Id == cityId);

				if (city == null)
				{
					_logger.LogInformation($"City with id {cityId} not found when accessing points of interest.");
					return NotFound();
				}

				return Ok(city.PointsOfInterest);

			}
			catch (Exception ex)
			{
				_logger.LogCritical(
					$"Exception while getting points of interest for city with city id {cityId}",
					ex);
				return StatusCode(500, "The server encountered an error while handling your request.");
			}
		}

		[HttpGet("{pointOfInterestId}", Name = "GetPointOfInterest")]
		public ActionResult<PointOfInterestDto> GetPointOfInterest(int cityId, int pointOfInterestId)
		{
			var city = _citiesDataStore.Cities.FirstOrDefault(c => c.Id == cityId);
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
			var city = _citiesDataStore.Cities.FirstOrDefault(c => cityId == c.Id);
			if (city == null)
			{
				return NotFound();
			}

			// demo - assigning ID - will need to be improved...
			var highPoiId = _citiesDataStore.Cities.SelectMany(c => c.PointsOfInterest).Max(p => p.Id);

			// create new POI object (not the creation one)
			var poiToAdd = new PointOfInterestDto()
			{
				Id = highPoiId + 1,
				Name = pointOfInterestForCreationDto.Name,
				Description = pointOfInterestForCreationDto.Description,
			};

			// add to list of poiStore
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
			var city = _citiesDataStore.Cities.FirstOrDefault(c => c.Id == cityId);

			if (city == null)
			{
				return NotFound();
			}

			// get poiStore
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

		[HttpPatch("{pointOfInterestId}")]
		public ActionResult PartiallyUpdatePointOfInterest(
			int cityId,
			int pointOfInterestId,
			JsonPatchDocument<PointOfInterestForUpdateDto> patchDocument)
		{
			var city = _citiesDataStore.Cities.FirstOrDefault(c => c.Id == cityId);
			if (city == null)
			{
				return NotFound();
			}

			var poiStore = city.PointsOfInterest.FirstOrDefault(p => p.Id == pointOfInterestId);
			if (poiStore == null)
			{
				return NotFound();
			}

			// create forUpdateDto type from the poiStore in the list
			var poiToPatch = new PointOfInterestForUpdateDto()
			{
				Name = poiStore.Name,
				Description = poiStore.Description,
			};

			// use builtin to patch?
			patchDocument.ApplyTo(poiToPatch, ModelState); // ModelState is referencing the JsonPatchDocument, which is not handled by the [ApiController], and has to be specified.

			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			// so the patch document itself is valid, but need to check if the point of interest is also valid.
			if (!TryValidateModel(poiToPatch))
			{
				return BadRequest(ModelState);
			}

			// update properties, as now the patch doc itself, along with the info it's applying to a POI object is acceptable.
			poiStore.Name = poiToPatch.Name;
			poiStore.Description = poiToPatch.Description;

			return NoContent();
		}

		[HttpDelete("{pointOfInterestId}")]
		public ActionResult DeletePointOfInterest(int cityId, int pointOfInterestId)
		{
			var city = _citiesDataStore.Cities.FirstOrDefault(c => c.Id == cityId);
			if (city == null)
			{
				return NotFound();
			}

			var poiToDelete = city.PointsOfInterest.FirstOrDefault(p => p.Id == pointOfInterestId);
			if (poiToDelete == null)
			{
				return NotFound();
			}

			city.PointsOfInterest.Remove(poiToDelete);

			_mailService.Send("Point of interest deleted",
				$"Point of interest {poiToDelete.Name} with id {poiToDelete.Id} was deleted at {DateTime.Now}");

			return NoContent();

		}

	}
}
