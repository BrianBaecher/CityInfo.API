using Asp.Versioning;
using AutoMapper;
using CityInfo.API.Models;
using CityInfo.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;


namespace CityInfo.API.Controllers
{
	[Route("api/v{version:apiVersion}/cities/{cityId}/pointsofinterest")]
	[Authorize(Policy = "MustBeFromCity")]
	[ApiController]
	[ApiVersion(2)]
	public class PointsOfInterestController : ControllerBase
	{
		private readonly ILogger<PointsOfInterestController> _logger;
		private readonly IMailService _mailService;
		private readonly ICityInfoRepository _cityInfoRepository;
		private readonly IMapper _mapper;

		public PointsOfInterestController(
			ILogger<PointsOfInterestController> logger,
			IMailService mailService,
			ICityInfoRepository cityInfoRepository,
			IMapper mapper)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_mailService = mailService ?? throw new ArgumentNullException(nameof(mailService));
			_cityInfoRepository = cityInfoRepository ?? throw new ArgumentNullException(nameof(cityInfoRepository));
			_mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
		}

		[HttpGet]
		public async Task<ActionResult<IEnumerable<PointOfInterestDto>>> GetPointsOfInterest(int cityId)
		{
			bool cityExists = await _cityInfoRepository.CityExistsAsync(cityId);

			if (!cityExists)
			{
				_logger.LogInformation($"City with id # {cityId} was not found when accessing points of interest");
				return NotFound();
			}

			// map to dto from entity
			var poiEntities = await _cityInfoRepository.GetPointsOfInterestForCityAsync(cityId);

			var result = _mapper.Map<IEnumerable<PointOfInterestDto>>(poiEntities);

			return Ok(result);
		}

		[HttpGet("{pointOfInterestId}", Name = "GetPointOfInterest")]
		public async Task<ActionResult<PointOfInterestDto>> GetPointOfInterest(int cityId, int pointOfInterestId)
		{
			var cityExists = await _cityInfoRepository.CityExistsAsync(cityId);

			if (!cityExists)
			{
				_logger.LogInformation($"City with id # {cityId} was not found when accessing point of interest with id # {pointOfInterestId}");
				return NotFound();
			}

			var poiEntity = await _cityInfoRepository.GetPointOfInterestAsync(cityId, pointOfInterestId);

			if (poiEntity == null)
			{
				_logger.LogInformation($"Point of interest with id # {pointOfInterestId} was not found in city # {cityId}'s list of POI");
				return NotFound();
			}

			// map to dto from entity
			var result = _mapper.Map<PointOfInterestDto>(poiEntity);

			return Ok(result);
		}

		[HttpPost]
		public async Task<ActionResult<PointOfInterestDto>> CreatePointOfInterest(
			int cityId,
			PointOfInterestForCreationDto pointOfInterestForCreationDto // is coming [FromBody], without being specified
			)
		{
			// check for valid city
			bool cityExists = await _cityInfoRepository.CityExistsAsync(cityId);

			if (!cityExists)
			{
				_logger.LogInformation($"City with id # {cityId} not found when attempting to add a point of interest");
				return BadRequest();
			}

			// db should automatically assign appropriate id to new Poi, as it is the POI table's primary key...
			var poiEntity = _mapper.Map<Entities.PointOfInterest>(pointOfInterestForCreationDto);

			await _cityInfoRepository.AddPointOfInterestForCity(cityId, poiEntity);

			await _cityInfoRepository.SaveChangesAsync();

			// map back to dto for response...

			var dtoToReturn = _mapper.Map<PointOfInterestDto>(poiEntity);

			return CreatedAtRoute("GetPointOfInterest", new
			{
				cityId = cityId,
				pointOfInterestId = dtoToReturn.Id
			},
			dtoToReturn);
		}

		[HttpPut("{pointOfInterestId}")]
		public async Task<ActionResult> UpdatePointOfInterest(int cityId, int pointOfInterestId, PointOfInterestForUpdateDto forUpdateDto)
		{
			// get city
			//var city = _citiesDataStore.Cities.FirstOrDefault(c => c.Id == cityId);
			bool cityExists = await _cityInfoRepository.CityExistsAsync(cityId);

			if (!cityExists)
			{
				return NotFound();
			}

			// get the poi
			var poi = await _cityInfoRepository.GetPointOfInterestAsync(cityId, pointOfInterestId);

			if (poi == null)
			{
				return NotFound();
			}

			// different usage of AutoMapper.Map, where the source object is mapped onto the target/destination object.
			_mapper.Map(forUpdateDto, poi);

			await _cityInfoRepository.SaveChangesAsync();

			return NoContent(); // could also return the POI, but not necessary as it's coming from client anyway...
		}

		[HttpPatch("{pointOfInterestId}")]
		public async Task<ActionResult> PartiallyUpdatePointOfInterest(
			int cityId,
			int pointOfInterestId,
			JsonPatchDocument<PointOfInterestForUpdateDto> patchDocument)
		{
			bool cityExists = await _cityInfoRepository.CityExistsAsync(cityId);

			if (!cityExists)
			{
				return NotFound();
			}

			var poiEntity = await _cityInfoRepository.GetPointOfInterestAsync(cityId, pointOfInterestId);
			if (poiEntity == null)
			{
				return NotFound();
			}

			// here need to map entity -> PointOfInterestDto
			var poiToPatch = _mapper.Map<PointOfInterestForUpdateDto>(poiEntity);

			// use builtin to patch
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

			// apply mapping to the entity
			_mapper.Map(poiToPatch, poiEntity);

			await _cityInfoRepository.SaveChangesAsync();

			return NoContent();
		}

		[HttpDelete("{pointOfInterestId}")]
		public async Task<ActionResult> DeletePointOfInterest(int cityId, int pointOfInterestId)
		{
			bool cityExists = await _cityInfoRepository.CityExistsAsync(cityId);

			if (!cityExists)
			{
				return NotFound();
			}

			var poiEntityToDelete = await _cityInfoRepository.GetPointOfInterestAsync(cityId, pointOfInterestId);
			if (poiEntityToDelete == null)
			{
				return NotFound();
			}

			_cityInfoRepository.DeletePointOfInterest(poiEntityToDelete);

			await _cityInfoRepository.SaveChangesAsync();

			_mailService.Send("Point of interest deleted",
				$"Point of interest {poiEntityToDelete.Name} with id {poiEntityToDelete.Id} was deleted at {DateTime.Now}");

			return NoContent();

		}

	}
}
