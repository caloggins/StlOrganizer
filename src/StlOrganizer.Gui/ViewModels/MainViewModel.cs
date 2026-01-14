using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using StlOrganizer.Library;
using StlOrganizer.Library.OperationSelection;
using StlOrganizer.Library.SystemAdapters;
using StlOrganizer.Library.SystemAdapters.AsyncWork;

namespace StlOrganizer.Gui.ViewModels;

public partial class MainViewModel : ObservableValidator
{
    private readonly ICancellationTokenSourceProvider cancellationTokenSourceProvider;
    private readonly IArchiveOperationSelector archiveOperationSelector;
    private CancellationTokenSource? cancellationToken;

    [ObservableProperty] private bool isBusy;

    [ObservableProperty] private int progress;

    [ObservableProperty]
    [Required(ErrorMessage = "Directory is required.")]
    private string selectedDirectory = string.Empty;

    [ObservableProperty] private ArchiveOperation selectedOperation;

    [ObservableProperty] private string statusMessage = string.Empty;

    [ObservableProperty] private string textFieldValue = string.Empty;

    [ObservableProperty] private string title = "Stl Organizer";

    public MainViewModel() : this(null!, null!)
    {
    }

    public MainViewModel(
        IArchiveOperationSelector archiveOperationSelector,
        ICancellationTokenSourceProvider cancellationTokenSourceProvider)
    {
        this.archiveOperationSelector = archiveOperationSelector;
        this.cancellationTokenSourceProvider = cancellationTokenSourceProvider;
        AvailableOperations =
        [
            ArchiveOperation.DecompressArchives,
            ArchiveOperation.CompressFolder,
            ArchiveOperation.ExtractImages
        ];
        SelectedOperation = ArchiveOperation.DecompressArchives;
        
        ValidateAllProperties();
        UpdateStatusMessageFromValidation();
    }

    public ObservableCollection<ArchiveOperation> AvailableOperations { get; }

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

            var result = await archiveOperationSelector.ExecuteOperationAsync(
                    SelectedOperation,
                    SelectedDirectory,
                    new Progress<OrganizerProgress>(),
                    cancellationToken.Token);
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