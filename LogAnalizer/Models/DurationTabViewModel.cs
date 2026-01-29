using System.Collections.ObjectModel;

namespace LogAnalizer.Models;

public class DurationTabViewModel
{
    public string Header { get; init; } = string.Empty;
    public string AddedLogLine { get; init; } = string.Empty;
    public string DurationLogLine { get; init; } = string.Empty;
    public TimeSpan Duration { get; init; }
    public DateTime? Timestamp { get; init; }
    public ObservableCollection<InteractionRecord> Interactions { get; } = new();
    public List<string> InteractionIds { get; init; } = new();
}
