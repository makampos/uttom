namespace Uttom.Domain.Results;

public class ResultResponse<T>
{
    public bool Success { get; private set; }
    public T? Data { get; private set; }
    public string? ErrorMessage { get; private set; }

    public static ResultResponse<T> SuccessResult(T data)
    {
        return new ResultResponse<T>
        {
            Success = true,
            Data = data,
            ErrorMessage = null
        };
    }

    public static ResultResponse<T> FailureResult(string errorMessage)
    {
        return new ResultResponse<T>
        {
            Success = false,
            Data = default,
            ErrorMessage = errorMessage
        };
    }
}