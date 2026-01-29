using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using LogAnalizer.Services;

namespace LogAnalizer;

public partial class MainWindow : Window
{
    private readonly MainViewModel _viewModel = new();

    public MainWindow()
    {
        InitializeComponent();
        DataContext = _viewModel;
    }

    private void Window_OnDragOver(object sender, System.Windows.DragEventArgs e)
    {
        e.Effects = System.Windows.DragDropEffects.None;
        if (e.Data.GetDataPresent(System.Windows.DataFormats.FileDrop))
        {
            e.Effects = System.Windows.DragDropEffects.Copy;
        }

        e.Handled = true;
    }

    private void Window_OnDrop(object sender, System.Windows.DragEventArgs e)
    {
        if (!e.Data.GetDataPresent(System.Windows.DataFormats.FileDrop))
        {
            return;
        }

        var items = (string[]?)e.Data.GetData(System.Windows.DataFormats.FileDrop);
        if (items is null || items.Length == 0)
        {
            return;
        }

        if (Directory.Exists(items[0]))
        {
            _viewModel.LogsFolder = items[0];
            _viewModel.StatusMessage = "Folder loaded. Click 'Search'.";
        }
    }

    private void SelectFolder_OnClick(object sender, RoutedEventArgs e)
    {
        using var dialog = new FolderBrowserDialog
        {
            Description = "Select a folder with logs",
            UseDescriptionForTitle = true,
            ShowNewFolderButton = false
        };

        if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        {
            _viewModel.LogsFolder = dialog.SelectedPath;
            _viewModel.StatusMessage = "Folder loaded. Click 'Search'.";
        }
    }

    private void Search_OnClick(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_viewModel.LogsFolder) || !Directory.Exists(_viewModel.LogsFolder))
        {
            _viewModel.StatusMessage = "Select a folder with logs.";
            return;
        }

        if (!TryParseDuration(_viewModel.DurationFromText, out var minDuration))
        {
            _viewModel.StatusMessage = "Invalid Duration 'from' format.";
            return;
        }

        if (!TryParseDuration(_viewModel.DurationToText, out var maxDuration))
        {
            _viewModel.StatusMessage = "Invalid Duration 'to' format.";
            return;
        }

        if (minDuration.HasValue && maxDuration.HasValue && minDuration > maxDuration)
        {
            _viewModel.StatusMessage = "Duration 'from' must be less than or equal to 'to'.";
            return;
        }

        var tabs = LogParser.ParseFolder(_viewModel.LogsFolder, minDuration, maxDuration);
        _viewModel.DurationTabs.Clear();
        foreach (var tab in tabs)
        {
            _viewModel.DurationTabs.Add(tab);
        }

        _viewModel.StatusMessage = $"Tabs found: {tabs.Count}.";
        _viewModel.SelectedDurationTab = _viewModel.DurationTabs.FirstOrDefault();
    }

    private static bool TryParseDuration(string? text, out TimeSpan? duration)
    {
        duration = null;
        if (string.IsNullOrWhiteSpace(text))
        {
            return true;
        }

        if (!TimeSpan.TryParse(text, CultureInfo.InvariantCulture, out var parsed))
        {
            return false;
        }

        duration = parsed;
        return true;
    }
}
