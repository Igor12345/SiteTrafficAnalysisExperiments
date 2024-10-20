namespace LogsAnalyzer.Exception;

public class IncorrectLogRecordsException : System.Exception
{
    public IncorrectLogRecordsException()
    {
    }

    public IncorrectLogRecordsException(string? message) : base(message)
    {
    }

    public IncorrectLogRecordsException(string? message, System.Exception? innerException) : base(message, innerException)
    {
    }
}