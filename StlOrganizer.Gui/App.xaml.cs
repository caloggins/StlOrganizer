using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using StlOrganizer.Gui.ViewModels;
using StlOrganizer.Library;
using StlOrganizer.Library.Compression;
using StlOrganizer.Library.Decompression;
using StlOrganizer.Library.ImageProcessing;
using StlOrganizer.Library.SystemFileAdapters;

namespace StlOrganizer.Gui;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
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
        services.AddSingleton<IZipArchiveFactory, ZipArchiveFactory>();
        services.AddSingleton<FileDecompressor>();
        services.AddSingleton<ImageOrganizer>();
        services.AddSingleton<FolderCompressor>();
        services.AddSingleton<OperationSelector>();
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