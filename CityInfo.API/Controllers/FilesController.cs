using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;

namespace CityInfo.API.Controllers
{
	[Route("api/files")]
	[ApiController]
	public class FilesController : ControllerBase
	{
		private readonly FileExtensionContentTypeProvider _extensionContentTypeProvider;

		public FilesController(
			FileExtensionContentTypeProvider extensionContentTypeProvider)
		{
			_extensionContentTypeProvider = extensionContentTypeProvider ?? throw new System.ArgumentNullException(nameof(extensionContentTypeProvider));
		}

		[HttpGet("{fileId}")]
		public ActionResult GetFile(string fileId)
		{
			// in reality, you wouldn't hardcode the file you're getting
			var path = "gantt-chart.pdf";

			// check file exists
			if (!System.IO.File.Exists(path))
			{
				return NotFound();
			}

			if (!_extensionContentTypeProvider.TryGetContentType(
				path, out var contentType))
			{
				contentType = "application/octet-stream";
			}

			var bytes = System.IO.File.ReadAllBytes(path);
			return File(bytes, contentType, Path.GetFileName(path));
		}
	}
}
