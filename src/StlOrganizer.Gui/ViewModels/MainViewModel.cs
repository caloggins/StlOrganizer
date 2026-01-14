using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using StlOrganizer.Library;
using StlOrganizer.Library.SystemAdapters;

namespace StlOrganizer.Gui.ViewModels;

public partial class MainViewModel : ObservableValidator
{
    private readonly ICancellationTokenSourceProvider cancellationTokenSourceProvider;
    private readonly IOperationSelector operationSelector;
    private CancellationTokenSource? cancellationToken;

    [ObservableProperty] private bool isBusy;

    [ObservableProperty] private int progress;

    [ObservableProperty]
    [Required(ErrorMessage = "Directory is required.")]
    private string selectedDirectory = string.Empty;

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
        
        ValidateAllProperties();
        UpdateStatusMessageFromValidation();
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

    partial void OnSelectedDirectoryChanged(string value)
    {
        ValidateProperty(value, nameof(SelectedDirectory));
        UpdateStatusMessageFromValidation();
    }

    private void UpdateStatusMessageFromValidation()
    {
        if (HasErrors)
        {
            var error = GetErrors(nameof(SelectedDirectory)).FirstOrDefault();
            StatusMessage = error?.ErrorMessage ?? "Validation error.";
        }
        else if (StatusMessage == "Directory is required." || string.IsNullOrWhiteSpace(StatusMessage) || StatusMessage == "Please select a directory first.")
        {
            StatusMessage = "Ready";
        }
    }

    [RelayCommand]
    private void Cancel()
    {
        cancellationToken?.Cancel();
    }

    [RelayCommand]
    private async Task ExecuteOperationAsync()
    {
        ValidateAllProperties();
        UpdateStatusMessageFromValidation();

        if (HasErrors)
        {
            return;
        }

        cancellationToken = cancellationTokenSourceProvider.Create();

        try
        {
            IsBusy = true;
            StatusMessage = $"Executing {SelectedOperation}...";

            var result =
                await operationSelector.ExecuteOperationAsync(SelectedOperation, SelectedDirectory, cancellationToken.Token);
            StatusMessage = result;
        }
        catch (OperationCanceledException)
        {
            StatusMessage = "Operation canceled.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
            cancellationToken.Dispose();
            cancellationToken = null;
        }
    }
    
    
}