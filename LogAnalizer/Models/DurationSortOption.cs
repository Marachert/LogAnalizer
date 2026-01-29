namespace LogAnalizer.Models;

public enum DurationSortType
{
    DateDescending,
    DateAscending,
    DurationDescending,
    DurationAscending
}

public sealed class DurationSortOption
{
    public DurationSortOption(string label, DurationSortType sortType)
    {
        Label = label;
        SortType = sortType;
    }

    public string Label { get; }
    public DurationSortType SortType { get; }
}
