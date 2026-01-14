using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using StlOrganizer.Gui.ViewModels;
using StlOrganizer.Library;
using StlOrganizer.Library.Compression;
using StlOrganizer.Library.Decompression;
using StlOrganizer.Library.ImageProcessing;
using StlOrganizer.Library.SystemAdapters;
using StlOrganizer.Library.SystemFileAdapters;

namespace StlOrganizer.Gui;

/// <summary>
///     Interaction logic for App.xaml
/// </summary>
public partial class App
{
    private readonly ServiceProvider serviceProvider;

    public App()
    {
        var services = new ServiceCollection();
        ConfigureServices(services);
        serviceProvider = services.BuildServiceProvider();
    }

    private void ConfigureServices(ServiceCollection services)
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .WriteTo.File("logs/app.log", rollingInterval: RollingInterval.Day)
            .CreateLogger();

        services.AddSingleton(Log.Logger);
        services.AddSingleton<IFileSystem, FileSystemAdapter>();
        services.AddSingleton<IFileOperations, FileOperationsAdapter>();
        services.AddSingleton<IDirectoryService, DirectoryServiceAdapter>();
        services.AddSingleton<IZipArchiveFactory, ZipArchiveFactory>();
        services.AddSingleton<IDecompressor, ZipFileAdapter>();
        services.AddSingleton<IFolderScanner, FolderScanner>();
        services.AddSingleton<IFolderFlattener, FolderFlattener>();
        services.AddSingleton<IDecompressionWorkflow, DecompressionWorkflow>();
        services.AddSingleton<IImageOrganizer, ImageOrganizer>();
        services.AddSingleton<IFolderCompressor, FolderCompressor>();
        services.AddSingleton<IOperationSelector, OperationSelector>();
        services.AddSingleton<ICancellationTokenSourceProvider, CancellationTokenSourceProvider>();
        services.AddTransient<MainViewModel>();
        services.AddTransient<MainWindow>();
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        var mainWindow = serviceProvider.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        base.OnExit(e);
        Log.CloseAndFlush();
        serviceProvider.Dispose();
    }
}