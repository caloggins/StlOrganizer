using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using StlOrganizer.Library;

namespace StlOrganizer.Gui.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly ICancellationTokenSourceProvider cancellationTokenSourceProvider;
    private readonly IOperationSelector operationSelector;

    [ObservableProperty] private bool isBusy;

    [ObservableProperty] private int progress;

    [ObservableProperty] private string selectedDirectory = string.Empty;

    [ObservableProperty] private FileOperation selectedOperation;

    [ObservableProperty] private string statusMessage = string.Empty;

    [ObservableProperty] private string textFieldValue = string.Empty;

    [ObservableProperty] private string title = "Stl Organizer";

    public MainViewModel() : this(null!, null!)
    {
    }

    public MainViewModel(
        IOperationSelector operationSelector,
        ICancellationTokenSourceProvider cancellationTokenSourceProvider)
    {
        this.operationSelector = operationSelector;
        this.cancellationTokenSourceProvider = cancellationTokenSourceProvider;
        AvailableOperations =
        [
            FileOperation.DecompressFiles,
            FileOperation.CompressFolder,
            FileOperation.ExtractImages
        ];
        SelectedOperation = FileOperation.DecompressFiles;
    }

    public ObservableCollection<FileOperation> AvailableOperations { get; }

    [RelayCommand]
    private void ChangeTitle()
    {
        Title = "Stl Organizer - Updated";
    }

    [RelayCommand]
    private void SelectDirectory()
    {
        var dialog = new OpenFolderDialog();

        if (dialog.ShowDialog() == true) SelectedDirectory = dialog.FolderName;
    }

    [RelayCommand]
    private async Task ExecuteOperationAsync()
    {
        if (string.IsNullOrWhiteSpace(SelectedDirectory))
        {
            StatusMessage = "Please select a directory first.";
            return;
        }

        using var tokenSource = cancellationTokenSourceProvider.Create();

        try
        {
            IsBusy = true;
            StatusMessage = $"Executing {SelectedOperation}...";

            var result =
                await operationSelector.ExecuteOperationAsync(SelectedOperation, SelectedDirectory, tokenSource.Token);
            StatusMessage = result;
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }
}