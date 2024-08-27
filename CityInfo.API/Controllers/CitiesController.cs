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

		public CitiesController(ICityInfoRepository cityInfoRepository, IMapper mapper)
		{
			_cityInfoRepository = cityInfoRepository ?? throw new ArgumentNullException(nameof(cityInfoRepository));
			_mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
		}

		[HttpGet]
		public async Task<ActionResult<IEnumerable<CityWithoutPointsOfInterestDto>>> GetCities()
		{
			var cities = await _cityInfoRepository.GetCitiesAsync();

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
		public async Task<IActionResult> GetCity(int id, bool includePointsOfInterest = false)
		{
			//var cityDto = _citiesDataStore.Cities.FirstOrDefault(c => c.Id == id);

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
