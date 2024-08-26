using System.ComponentModel.DataAnnotations;

namespace CityInfo.API.Models
{
	public class PointOfInterestForUpdateDto
	{
		[Required(ErrorMessage = "A name value is required for submissions.")]
		[MaxLength(50)]
		public string Name { get; set; } = string.Empty;

		[MaxLength(200)]
		public string? Description { get; set; }
	}
}
