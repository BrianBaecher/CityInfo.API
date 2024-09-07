using Asp.Versioning;
using AutoMapper;
using CityInfo.API.Models;
using CityInfo.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace CityInfo.API.Controllers
{
	[ApiController]
	[Authorize]
	[Route("api/v{version:apiVersion}/cities")]
	[ApiVersion(1)]
	[ApiVersion(2)]
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

			var (cityEntities, paginationMetadata) = await _cityInfoRepository.GetCitiesAsync(name, searchQuery, pageNumber, pageSize);

			Response.Headers.Append("X-Pagination",
				JsonSerializer.Serialize(paginationMetadata));

			return Ok(_mapper.Map<IEnumerable<CityWithoutPointsOfInterestDto>>(cityEntities)); // type coming from DB are Entities.City, but to use them we want the type specified in the method definition...

		}

		/// <summary>
		/// Get a city by cityId
		/// </summary>
		/// <param name="cityId">The cityId of the city to get</param>
		/// <param name="includePointsOfInterest">whether or not to include the city's points of interest</param>
		/// <returns>a city, with or without a list of its points of interest</returns>
		/// <response code = "200">Returns the requested city</response>
		/// <response code = "404">City with provided cityId does not exist in database</response>
		[HttpGet("{cityId}")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		public async Task<IActionResult> GetCity(
			int cityId,
			bool includePointsOfInterest = false
			)
		{
			var cityEntity = await _cityInfoRepository.GetCityAsync(cityId, includePointsOfInterest);

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
