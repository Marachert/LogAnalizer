using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using LogAnalizer.Models;

namespace LogAnalizer;

public class MainViewModel : INotifyPropertyChanged
{
    private string? _logsFolder;
    private string? _durationFromText = "00:00:00";
    private string? _durationToText = "00:00:00";
    private string? _statusMessage;
    private DurationTabViewModel? _selectedDurationTab;
    private DurationSortOption? _selectedDurationSortOption;

    public event PropertyChangedEventHandler? PropertyChanged;

    public ObservableCollection<DurationTabViewModel> DurationTabs { get; } = new();
    public ObservableCollection<DurationSortOption> DurationSortOptions { get; } = new();

    public MainViewModel()
    {
        DurationSortOptions.Add(new DurationSortOption("Date (newest first)", DurationSortType.DateDescending));
        DurationSortOptions.Add(new DurationSortOption("Date (oldest first)", DurationSortType.DateAscending));
        DurationSortOptions.Add(new DurationSortOption("Duration (longest first)", DurationSortType.DurationDescending));
        DurationSortOptions.Add(new DurationSortOption("Duration (shortest first)", DurationSortType.DurationAscending));
        _selectedDurationSortOption = DurationSortOptions.FirstOrDefault();
    }
    public string? LogsFolder
    {
        get => _logsFolder;
        set => SetField(ref _logsFolder, value);
    }

    public string? DurationFromText
    {
        get => _durationFromText;
        set => SetField(ref _durationFromText, value);
    }

    public string? DurationToText
    {
        get => _durationToText;
        set => SetField(ref _durationToText, value);
    }

    public string? StatusMessage
    {
        get => _statusMessage;
        set => SetField(ref _statusMessage, value);
    }

    public DurationTabViewModel? SelectedDurationTab
    {
        get => _selectedDurationTab;
        set => SetField(ref _selectedDurationTab, value);
    }

    public DurationSortOption? SelectedDurationSortOption
    {
        get => _selectedDurationSortOption;
        set => SetField(ref _selectedDurationSortOption, value);
    }

    private void SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (Equals(field, value))
        {
            return;
        }

        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
