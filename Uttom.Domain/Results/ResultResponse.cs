using System.Text.Json.Serialization;

namespace Uttom.Domain.Results;

public class ResultResponse<T>
{
    [JsonConstructor]
    private ResultResponse(bool success, T? data, string? errorMessage)
    {
        Success = success;
        Data = data;
        ErrorMessage = errorMessage;
    }

    public bool Success { get; private set; }
    public T? Data { get; private set; }
    public string? ErrorMessage { get; private set; }

    public static ResultResponse<T> SuccessResult(T data)
    {
        return new ResultResponse<T>(true, data, null);
    }

    public static ResultResponse<T> FailureResult(string errorMessage)
    {
        return new ResultResponse<T>(false, default, errorMessage);
    }
}