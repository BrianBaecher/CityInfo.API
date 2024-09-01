namespace CityInfo.API.Services
{
	public class PaginationMetadata
	{
		public int TotalItemCount { get; set; }

		public int TotalPageCount { get; set; }

		public int PageSize { get; set; }

		public int CurrentPage { get; set; }

		public PaginationMetadata(int totalItemCount, int pagesize, int currentPage)
		{
			TotalItemCount = totalItemCount;
			PageSize = pagesize;
			CurrentPage = currentPage;
			TotalPageCount = (int)Math.Ceiling(totalItemCount / (double)pagesize);

		}
	}
}
