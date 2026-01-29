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

        if (!TimeSpan.TryParse(_viewModel.DurationFilterText, CultureInfo.InvariantCulture, out var minDuration))
        {
            _viewModel.StatusMessage = "Invalid Duration format.";
            return;
        }

        var tabs = LogParser.ParseFolder(_viewModel.LogsFolder, minDuration);
        _viewModel.DurationTabs.Clear();
        foreach (var tab in tabs)
        {
            _viewModel.DurationTabs.Add(tab);
        }

        _viewModel.StatusMessage = $"Tabs found: {tabs.Count}.";
        _viewModel.SelectedDurationTab = _viewModel.DurationTabs.FirstOrDefault();
    }
}
