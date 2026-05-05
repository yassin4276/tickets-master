namespace Ticketing.Application.Common.Responses;

public class ApiPagedResponse<T>
{
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public List<T> Items { get; set; } = new List<T>();
}