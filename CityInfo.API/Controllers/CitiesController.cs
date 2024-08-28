using AutoMapper;
using CityInfo.API.Models;
using CityInfo.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace CityInfo.API.Controllers
{
	[ApiController]
	[Route("api/cities")]
	public class CitiesController : ControllerBase
	{
		private readonly ICityInfoRepository _cityInfoRepository;
		private readonly IMapper _mapper;
		const int MAX_CITIES_PAGE_SIZE = 20;

		public CitiesController(
			ICityInfoRepository cityInfoRepository,
			IMapper mapper
			)
		{
			_cityInfoRepository = cityInfoRepository ?? throw new ArgumentNullException(nameof(cityInfoRepository));
			_mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
		}

		[HttpGet]
		public async Task<ActionResult<IEnumerable<CityWithoutPointsOfInterestDto>>> GetCities(
			[FromQuery] string? name,
			[FromQuery] string? searchQuery,
			[FromQuery] int pageNumber = 1,
			[FromQuery] int pageSize = 10
			)
		{
			if (pageSize > MAX_CITIES_PAGE_SIZE)
			{
				pageSize = MAX_CITIES_PAGE_SIZE;
			}

			var cities = await _cityInfoRepository.GetCitiesAsync(name, searchQuery, pageNumber, pageSize);

			if (cities == null || cities.Count() == 0)
			{
				return NotFound();
			}

			// WITHOUT AUTOMAPPER
			//var result = new List<CityWithoutPointsOfInterestDto>();

			//foreach (var cityEntity in cities)
			//{
			//	var dto = new CityWithoutPointsOfInterestDto
			//	{
			//		Id = cityEntity.Id,
			//		Name = cityEntity.Name,
			//		Description = cityEntity.Description,
			//	};
			//	result.Add(dto);
			//}

			// WITH AUTOMAPPER
			return Ok(_mapper.Map<IEnumerable<CityWithoutPointsOfInterestDto>>(cities)); // type coming from DB are Entities.City, but to use them we want the type specified in the method definition...

		}

		[HttpGet("{id}")]
		public async Task<IActionResult> GetCity(
			int id,
			bool includePointsOfInterest = false
			)
		{
			var cityEntity = await _cityInfoRepository.GetCityAsync(id, includePointsOfInterest);

			if (cityEntity == null)
			{
				return NotFound();
			}

			if (includePointsOfInterest)
			{
				return Ok(_mapper.Map<CityDto>(cityEntity));
			}
			else
			{
				return Ok(_mapper.Map<CityWithoutPointsOfInterestDto>(cityEntity));
			}

		}
	}
}
