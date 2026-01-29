namespace LogAnalizer.Models;

public class InteractionRecord
{
    public int Index { get; init; }
    public string InteractionId { get; init; } = string.Empty;
    public string InteractionType { get; init; } = string.Empty;
    public string RecordingProfileId { get; init; } = string.Empty;
    public string Date { get; init; } = string.Empty;
    public string MetaDataFileName { get; init; } = string.Empty;
    public string Contents { get; init; } = string.Empty;
    public string TranscriptFileName { get; init; } = string.Empty;
    public string RetentionDate { get; init; } = string.Empty;
}
