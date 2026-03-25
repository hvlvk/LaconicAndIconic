namespace LaconicAndIconic.BLL.Models;

public class Result
{
    public bool IsSuccess { get; }
    public string? ErrorMessage { get; }

    protected Result(bool isSuccess, string? errorMessage)
    {
        IsSuccess = isSuccess;
        ErrorMessage = errorMessage;
    }

    public static Result Success() => new(true, null);
    public static Result Failure(string errorMessage) => new(false, errorMessage);
}

public sealed class Result<T> : Result
{
    public T? Value { get; }

    private Result(bool isSuccess, T? value, string? errorMessage)
        : base(isSuccess, errorMessage)
    {
        Value = value;
    }

#pragma warning disable CA1000 // Factory methods on generic types require type argument at call site
    public static Result<T> Success(T value) => new(true, value, null);
    public static new Result<T> Failure(string errorMessage) => new(false, default, errorMessage);
#pragma warning restore CA1000
}
