namespace Ticketing.Application.Common.Responses;

public class ApiResponse<T>
{
    public bool Success { get; set; }

    public string? Message { get; set; }

    public T? Data { get; set; }

    public List<string>? Errors { get; set; }

    public static ApiResponse<T> Ok(T data, string? message = null)
    {
        return new ApiResponse<T>
        {
            Success = true,
            Message = message,
            Data = data,
            Errors = null
        };
    }

    public static ApiResponse<T> Fail(List<string> errors, string? message = null)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = message,
            Data = default,
            Errors = errors
        };
    }

    public static ApiResponse<T> Fail(string error, string? message = null)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = message,
            Data = default,
            Errors = new List<string> { error }
        };
    }
}