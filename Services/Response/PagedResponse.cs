// Services.Dto.Api/PagedResponse.cs
using BusinessObjects.Dto.ProductForSkinType;

namespace Services.Response
{
    public class PagedResponse<TItem>
    {
        public IEnumerable<TItem> Items { get; set; }
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    }
}