using CityInfo.API.Models;
using Microsoft.AspNetCore.Mvc;

namespace CityInfo.API.Controllers
{
	[ApiController]
	[Route("api/cities")]
	public class CitiesController : ControllerBase
	{
		private readonly CitiesDataStore _citiesDataStore;

		public CitiesController(CitiesDataStore citiesDataStore)
		{
			_citiesDataStore = citiesDataStore;
		}

		[HttpGet]
		public ActionResult<List<CityDto>> GetCities()
		{
			//return new JsonResult(
			//	CitiesDataStore.Current.Cities
			//	);
			return Ok(_citiesDataStore.Cities);
		}

		[HttpGet("{id}")]
		public ActionResult<CityDto> GetCity(int id)
		{
			var cityDto = _citiesDataStore.Cities.FirstOrDefault(c => c.Id == id);
			if (cityDto != null)
			{
				//return new JsonResult(cityDto);
				return new OkObjectResult(cityDto);
			}
			else
			{
				//return new JsonResult(string.Empty);
				return new NotFoundResult();
			}
		}
	}
}
