namespace LogAnalizer.Models;

public class PdfAnalysisResult
{
    public int Index { get; init; }
    public string InteractionId { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string Evidence { get; init; } = string.Empty;
}
