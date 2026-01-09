using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StlOrganizer.Library;

namespace StlOrganizer.Gui.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly FileDecompressor fileDecompressor = new();

    [ObservableProperty]
    private string title = "Stl Organizer";

    [ObservableProperty]
    private string textFieldValue = string.Empty;

    [ObservableProperty]
    private string selectedDirectory = string.Empty;

    [ObservableProperty]
    private bool isDecompressing;

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
    private async Task DecompressFilesAsync()
    {
        if (string.IsNullOrWhiteSpace(SelectedDirectory))
        {
            StatusMessage = "Please select a directory first.";
            return;
        }

        try
        {
            IsDecompressing = true;
            StatusMessage = "Decompressing files...";

            var extractedFiles = await fileDecompressor.ScanAndDecompressAsync(SelectedDirectory);
            var fileCount = extractedFiles.Count();

            StatusMessage = $"Successfully extracted {fileCount} file(s).";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
        finally
        {
            IsDecompressing = false;
        }
    }
}
