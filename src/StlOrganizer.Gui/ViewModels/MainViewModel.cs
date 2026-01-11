using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StlOrganizer.Library;

namespace StlOrganizer.Gui.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly IOperationSelector operationSelector;

    public MainViewModel(IOperationSelector operationSelector)
    {
        this.operationSelector = operationSelector;
        AvailableOperations =
        [
            OperationType.FileDecompressor,
            OperationType.FolderCompressor,
            OperationType.ImageOrganizer
        ];
        SelectedOperation = OperationType.FileDecompressor;
    }

    public ObservableCollection<OperationType> AvailableOperations { get; }

    [ObservableProperty]
    private string title = "Stl Organizer";

    [ObservableProperty]
    private string textFieldValue = string.Empty;

    [ObservableProperty]
    private string selectedDirectory = string.Empty;

    [ObservableProperty]
    private OperationType selectedOperation;

    [ObservableProperty]
    private bool isExecuting;

    [ObservableProperty]
    private string statusMessage = string.Empty;

    [RelayCommand]
    private void ChangeTitle()
    {
        Title = "Stl Organizer - Updated";
    }

    [RelayCommand]
    private void SelectDirectory()
    {
        var dialog = new Microsoft.Win32.OpenFolderDialog();
        if (dialog.ShowDialog() == true)
        {
            SelectedDirectory = dialog.FolderName;
        }
    }

    [RelayCommand]
    private async Task ExecuteOperationAsync()
    {
        if (string.IsNullOrWhiteSpace(SelectedDirectory))
        {
            StatusMessage = "Please select a directory first.";
            return;
        }

        try
        {
            IsExecuting = true;
            StatusMessage = $"Executing {SelectedOperation}...";

            var result = await operationSelector.ExecuteOperationAsync(SelectedOperation, SelectedDirectory);
            StatusMessage = result;
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
        finally
        {
            IsExecuting = false;
        }
    }
}
