namespace Infrastructure;

public record struct Result<T>(bool Success, T Value, string ErrorMessage)
{
    public static Result<T> Ok(T value) => new(true, value, "");
    public static Result<T> Error(string message) => new(false, default, message);

    public bool IsError => !Success;
}