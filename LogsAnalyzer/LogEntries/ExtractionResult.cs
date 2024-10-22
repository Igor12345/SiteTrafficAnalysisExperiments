namespace LogsAnalyzer.LogEntries;

public record struct ExtractionResult(bool Success, int LinesNumber, int StartRemainingBytes, string Message)
{
    public static ExtractionResult Ok(int linesNumber, int startRemainingBytes) =>
        new(true, linesNumber, startRemainingBytes, "");

    public static ExtractionResult Error(string message) => new(false, -1, -1, message);
}