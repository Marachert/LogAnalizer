using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using LogAnalizer.Models;

namespace LogAnalizer.Services;

public static class LogParser
{
    private static readonly Regex DurationRegex = new(@"Duration:\s*(?<duration>\d{2}:\d{2}:\d{2}(?:\.\d+)?)", RegexOptions.Compiled);
    private static readonly Regex ArchivedRegex = new(@"Archived\s+(?<count>\d+)", RegexOptions.Compiled);
    private static readonly Regex AddedRegex = new(@"Added\s+(?<count>\d+)\s+interactions for archiving\.\s*Interaction ID's:\s*(?<ids>.+)", RegexOptions.Compiled);
    private static readonly Regex TimestampRegex = new(@"^(?<timestamp>\d{4}-\d{2}-\d{2}\s+\d{2}:\d{2}:\d{2}\.\d{3})", RegexOptions.Compiled);
    private static readonly Regex InteractionIdRegex = new(@"[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}", RegexOptions.Compiled);
    private static readonly Regex KeyValueRegex = new(@"(?<key>\w+)=\s*(?<value>.+?)(?=,\s\w+=|$)", RegexOptions.Compiled);

    public static List<DurationTabViewModel> ParseFolder(string folderPath, TimeSpan? minDuration, TimeSpan? maxDuration)
    {
        var tabs = new List<DurationTabViewModel>();

        foreach (var file in Directory.EnumerateFiles(folderPath, "*.log", SearchOption.AllDirectories))
        {
            var lines = File.ReadAllLines(file);
            if (lines.Length == 0)
            {
                continue;
            }

            var successLogsById = BuildSuccessLogIndex(lines);
            var addedLines = FindAddedLines(lines);

            foreach (var line in lines)
            {
                var durationMatch = DurationRegex.Match(line);
                if (!durationMatch.Success)
                {
                    continue;
                }

                if (!TimeSpan.TryParse(durationMatch.Groups["duration"].Value, CultureInfo.InvariantCulture, out var duration))
                {
                    continue;
                }

                if (minDuration.HasValue && duration < minDuration.Value)
                {
                    continue;
                }

                if (maxDuration.HasValue && duration > maxDuration.Value)
                {
                    continue;
                }

                var archivedMatch = ArchivedRegex.Match(line);
                if (!archivedMatch.Success)
                {
                    continue;
                }

                if (!int.TryParse(archivedMatch.Groups["count"].Value, out var archivedCount))
                {
                    continue;
                }

                var addedLine = addedLines.FirstOrDefault(item => item.Count == archivedCount);
                if (addedLine.Line is null)
                {
                    continue;
                }

                var interactionIds = InteractionIdRegex.Matches(addedLine.Line)
                    .Select(match => match.Value)
                    .Distinct()
                    .ToList();

                var interactions = new List<InteractionRecord>();
                var index = 1;
                foreach (var interactionId in interactionIds)
                {
                    if (!successLogsById.TryGetValue(interactionId, out var successLine))
                    {
                        continue;
                    }

                    var parsed = ParseSuccessLine(successLine, interactionId, index);
                    interactions.Add(parsed);
                    index++;
                }

                var header = BuildHeader(line, archivedCount, duration);
                var tab = new DurationTabViewModel
                {
                    Header = header,
                    AddedLogLine = addedLine.Line ?? string.Empty,
                    DurationLogLine = line,
                    InteractionIds = interactionIds
                };

                foreach (var interaction in interactions)
                {
                    tab.Interactions.Add(interaction);
                }

                tabs.Add(tab);
            }
        }

        return tabs;
    }

    private static Dictionary<string, string> BuildSuccessLogIndex(string[] lines)
    {
        var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var line in lines)
        {
            if (!line.Contains("Successfully archived:", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var idMatch = Regex.Match(line, @"InteractionId=(?<id>[0-9a-fA-F-]{36})");
            if (!idMatch.Success)
            {
                continue;
            }

            var id = idMatch.Groups["id"].Value;
            if (!map.ContainsKey(id))
            {
                map[id] = line;
            }
        }

        return map;
    }

    private static List<(int Count, string? Line)> FindAddedLines(string[] lines)
    {
        var results = new List<(int Count, string? Line)>();
        foreach (var line in lines)
        {
            var match = AddedRegex.Match(line);
            if (!match.Success)
            {
                continue;
            }

            if (!int.TryParse(match.Groups["count"].Value, out var count))
            {
                continue;
            }

            results.Add((count, line));
        }

        return results;
    }

    private static InteractionRecord ParseSuccessLine(string line, string interactionId, int index)
    {
        var dataSection = line.Contains("Successfully archived:")
            ? line.Split("Successfully archived:", 2, StringSplitOptions.TrimEntries)[1]
            : line;

        var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (Match match in KeyValueRegex.Matches(dataSection))
        {
            values[match.Groups["key"].Value] = match.Groups["value"].Value.Trim();
        }

        values.TryGetValue("InteractionType", out var interactionType);
        values.TryGetValue("RecordingProfileId", out var recordingProfileId);
        values.TryGetValue("Date", out var dateValue);
        values.TryGetValue("MetaDataFileName", out var metadataFileName);
        values.TryGetValue("Contents", out var contents);
        values.TryGetValue("TranscriptFileName", out var transcriptFileName);
        values.TryGetValue("RetentionDate", out var retentionDate);

        return new InteractionRecord
        {
            Index = index,
            InteractionId = interactionId,
            InteractionType = interactionType ?? string.Empty,
            RecordingProfileId = recordingProfileId ?? string.Empty,
            Date = dateValue ?? string.Empty,
            MetaDataFileName = metadataFileName ?? string.Empty,
            Contents = contents ?? string.Empty,
            TranscriptFileName = transcriptFileName ?? string.Empty,
            RetentionDate = retentionDate ?? string.Empty
        };
    }

    private static string BuildHeader(string durationLine, int archivedCount, TimeSpan duration)
    {
        var timestamp = "";
        var match = TimestampRegex.Match(durationLine);
        if (match.Success)
        {
            timestamp = match.Groups["timestamp"].Value;
        }

        return $"Archived {archivedCount} | {timestamp} | Duration {duration}";
    }
}
