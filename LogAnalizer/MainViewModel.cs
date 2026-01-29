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
    private string? _pdfFilePath;
    private string? _pdfStatusMessage;

    public event PropertyChangedEventHandler? PropertyChanged;

    public ObservableCollection<DurationTabViewModel> DurationTabs { get; } = new();
    public ObservableCollection<PdfAnalysisResult> PdfAnalysisResults { get; } = new();

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

    public string? PdfFilePath
    {
        get => _pdfFilePath;
        set => SetField(ref _pdfFilePath, value);
    }

    public string? PdfStatusMessage
    {
        get => _pdfStatusMessage;
        set => SetField(ref _pdfStatusMessage, value);
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
