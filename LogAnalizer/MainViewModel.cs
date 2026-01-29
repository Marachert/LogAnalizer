using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using LogAnalizer.Models;

namespace LogAnalizer;

public class MainViewModel : INotifyPropertyChanged
{
    private string? _logsFolder;
    private string? _durationFilterText = "00:00:00";
    private string? _statusMessage;
    private DurationTabViewModel? _selectedDurationTab;

    public event PropertyChangedEventHandler? PropertyChanged;

    public ObservableCollection<DurationTabViewModel> DurationTabs { get; } = new();
    public string? LogsFolder
    {
        get => _logsFolder;
        set => SetField(ref _logsFolder, value);
    }

    public string? DurationFilterText
    {
        get => _durationFilterText;
        set => SetField(ref _durationFilterText, value);
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
