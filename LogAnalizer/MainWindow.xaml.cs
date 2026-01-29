using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Forms;
using LogAnalizer.Models;
using LogAnalizer.Services;
using Microsoft.Win32;

namespace LogAnalizer;

public partial class MainWindow : Window
{
    private readonly MainViewModel _viewModel = new();
    private List<string> _pdfLines = new();

    public MainWindow()
    {
        InitializeComponent();
        DataContext = _viewModel;
        _viewModel.PropertyChanged += ViewModelOnPropertyChanged;
    }

    private void ViewModelOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MainViewModel.SelectedDurationTab))
        {
            UpdatePdfAnalysis();
        }
    }

    private void Window_OnDragOver(object sender, DragEventArgs e)
    {
        e.Effects = DragDropEffects.None;
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            e.Effects = DragDropEffects.Copy;
        }

        e.Handled = true;
    }

    private void Window_OnDrop(object sender, DragEventArgs e)
    {
        if (!e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            return;
        }

        var items = (string[]?)e.Data.GetData(DataFormats.FileDrop);
        if (items is null || items.Length == 0)
        {
            return;
        }

        if (Directory.Exists(items[0]))
        {
            _viewModel.LogsFolder = items[0];
            _viewModel.StatusMessage = "Папка загружена. Нажмите 'Найти'.";
        }
    }

    private void SelectFolder_OnClick(object sender, RoutedEventArgs e)
    {
        using var dialog = new FolderBrowserDialog
        {
            Description = "Выберите папку с логами",
            UseDescriptionForTitle = true,
            ShowNewFolderButton = false
        };

        if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        {
            _viewModel.LogsFolder = dialog.SelectedPath;
            _viewModel.StatusMessage = "Папка загружена. Нажмите 'Найти'.";
        }
    }

    private void Search_OnClick(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_viewModel.LogsFolder) || !Directory.Exists(_viewModel.LogsFolder))
        {
            _viewModel.StatusMessage = "Выберите папку с логами.";
            return;
        }

        if (!TimeSpan.TryParse(_viewModel.DurationFilterText, CultureInfo.InvariantCulture, out var minDuration))
        {
            _viewModel.StatusMessage = "Некорректный формат Duration.";
            return;
        }

        var tabs = LogParser.ParseFolder(_viewModel.LogsFolder, minDuration);
        _viewModel.DurationTabs.Clear();
        foreach (var tab in tabs)
        {
            _viewModel.DurationTabs.Add(tab);
        }

        _viewModel.StatusMessage = $"Найдено вкладок: {tabs.Count}.";
        _viewModel.SelectedDurationTab = _viewModel.DurationTabs.FirstOrDefault();
        UpdatePdfAnalysis();
    }

    private void LoadPdf_OnClick(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Filter = "PDF files (*.pdf)|*.pdf",
            Title = "Выберите PDF файл"
        };

        if (dialog.ShowDialog() != true)
        {
            return;
        }

        _viewModel.PdfFilePath = dialog.FileName;
        _viewModel.PdfStatusMessage = "Чтение PDF...";

        try
        {
            _pdfLines = PdfAnalyzer.ExtractLines(dialog.FileName);
            _viewModel.PdfStatusMessage = $"PDF загружен. Строк: {_pdfLines.Count}.";
        }
        catch (Exception ex)
        {
            _pdfLines = new List<string>();
            _viewModel.PdfStatusMessage = $"Ошибка PDF: {ex.Message}";
        }

        UpdatePdfAnalysis();
    }

    private void UpdatePdfAnalysis()
    {
        _viewModel.PdfAnalysisResults.Clear();
        var selected = _viewModel.SelectedDurationTab;
        if (selected is null || _pdfLines.Count == 0)
        {
            return;
        }

        var results = PdfAnalyzer.Analyze(selected.Interactions, _pdfLines);
        foreach (var result in results)
        {
            _viewModel.PdfAnalysisResults.Add(result);
        }
    }
}
