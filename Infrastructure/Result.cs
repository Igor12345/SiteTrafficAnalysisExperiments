namespace Infrastructure;

public record Result<T>
{
    public static Result<T> Ok(T value) => new Success<T>(value);
    public static Result<T> Error(string message) => new Failure<T>(message);

    public Result<TU> Bind<TU>(Func<T, Result<TU>> func)
    {
        if (func == null) throw new ArgumentNullException(nameof(func));
        try
        {
            var result = this switch
            {
                Failure<T> failure => Result<TU>.Error(failure.ErrorMessage),
                Success<T> success => func(success.Value),
                _ => throw new ArgumentOutOfRangeException()
            };
            return result;
        }
        catch (Exception e)
        {
            return Result<TU>.Error(e.Message);
        }
    }

    public Result<T> Tap(Action<T> action)
    {
        if (action == null) throw new ArgumentNullException(nameof(action));
        try
        {
            return this switch
            {
                Failure<T> => this,
                Success<T> success => ExecuteActionOn(success),
                _ => throw new ArgumentOutOfRangeException()
            };
            Result<T> ExecuteActionOn(Success<T> value)
            {
                action(value.Value);
                return value;
            }
        }
        catch (Exception e)
        {
            return Error(e.Message);
        }
    }

    public void OnSuccess(Action<T> action)
    {
        switch (this)
        {
            case Failure<T>:
                break;
            case Success<T> success:
                action(success.Value);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public TU Execute<TU>(Func<T, TU> onSuccess, Func<string, TU> onFailure)
    {
        return this switch
        {
            Failure<T> failure => onFailure(failure.ErrorMessage),
            Success<T> success => onSuccess(success.Value),
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}

public sealed record Success<T>(T Value) : Result<T>
{
}

public sealed record Failure<T>(string ErrorMessage) : Result<T>
{
}