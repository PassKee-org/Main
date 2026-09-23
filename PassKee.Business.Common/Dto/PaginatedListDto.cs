using System.Text.Json.Serialization;
using Api.Requests.Abstractions;
using PassKee.Business.Common.Constants;

namespace PassKee.Business.Common.Dto
{
    public class PaginatedListDto<TItem> : IResponse
    {
        [JsonPropertyName("items")]
        public ICollection<TItem> Items { get; set; }

        [JsonPropertyName("totalPages")]
        public int TotalPages { get; set; }

        [JsonPropertyName("pageSize")]
        public int PageSize { get; set; }

        [JsonPropertyName("totalCount")]
        public int TotalCount { get; set; }

        [JsonPropertyName("isHasMore")]
        public bool IsHasMore
        {
            get => PageSize <= Items.Count;
        }

        public PaginatedListDto()
        {
        }

        public PaginatedListDto(
            ICollection<TItem> responseList,
            int totalItems = 0,
            int pageSize = GlobalConstants.ListPageSize
        )
        {
            TotalCount = totalItems;
            Items = responseList;
            PageSize = pageSize;
            decimal totalPages = (decimal)totalItems / pageSize;
            TotalPages = (int)Math.Ceiling(totalPages);
        }
    }
}
