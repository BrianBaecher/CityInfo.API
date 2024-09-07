using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;


namespace CityInfo.API.Controllers
{
	[Route("api/v{version:apiVersion}/files")]
	[Authorize]
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
		[ApiVersion(0.1, Deprecated = true)]
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
		[HttpPost]
		public async Task<ActionResult> CreateFile(IFormFile file)
		{
			// validating input file. Limit size of uploads.
			// for demo purposes, he's only allowing pdf files to be uploaded.
			// also in real-world scenario, store uploaded files to a seperate dir, that does not have execute capability.
			if (file.Length == 0 || file.Length > 20971520 || file.ContentType != "application/pdf")
			{
				return BadRequest("Invalid File");
			}

			var path = Path.Combine(
				Directory.GetCurrentDirectory(),
				$"uploaded_file_{Guid.NewGuid()}.pdf");

			using (var stream = new FileStream(path, FileMode.Create))
			{
				await file.CopyToAsync(stream);
			}

			return Ok("File uploaded");
		}
	}
}
