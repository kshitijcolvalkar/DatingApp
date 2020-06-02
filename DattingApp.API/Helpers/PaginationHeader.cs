namespace DattingApp.API.Helpers
{
    public class PaginationHeader
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }
        
        public PaginationHeader(int pageNumber, int pageSize, int totalItems, int totalPages)
        {
            this.PageNumber = pageNumber;
            this.PageSize = pageSize;
            this.TotalItems = totalItems;
            this.TotalPages = totalPages;
        }
    }
}