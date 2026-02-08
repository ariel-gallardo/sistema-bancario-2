namespace BankingApp.SharedKernel.Responses;

public sealed class ErrorResponse
{
    public required string Message { get; set; }
    public string? TraceId { get; set; }
    public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;
    public IDictionary<string, string[]>? Errors { get; set; }
}
