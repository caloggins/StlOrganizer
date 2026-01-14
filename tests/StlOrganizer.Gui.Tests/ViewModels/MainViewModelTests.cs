using FakeItEasy;
using Shouldly;
using StlOrganizer.Gui.ViewModels;
using StlOrganizer.Library;
using StlOrganizer.Library.OperationSelection;
using StlOrganizer.Library.SystemAdapters;
using StlOrganizer.Library.SystemAdapters.AsyncWork;

namespace StlOrganizer.Gui.Tests.ViewModels;

public class MainViewModelTests
{
    private readonly ICancellationTokenSourceProvider cancellationTokenSourceProvider;
    private readonly IArchiveOperationSelector archiveOperationSelector;
    private readonly MainViewModel viewModel;

    public MainViewModelTests()
    {
        archiveOperationSelector = A.Fake<IArchiveOperationSelector>();
        cancellationTokenSourceProvider = A.Fake<ICancellationTokenSourceProvider>();
        A.CallTo(() => cancellationTokenSourceProvider.Create()).ReturnsLazily(() => new CancellationTokenSource());
        viewModel = new MainViewModel(archiveOperationSelector, cancellationTokenSourceProvider);
    }

    [Fact]
    public void Constructor_InitializesProperties()
    {
        viewModel.Title.ShouldBe("Stl Organizer");
        viewModel.TextFieldValue.ShouldBe(string.Empty);
        viewModel.SelectedDirectory.ShouldBe(string.Empty);
        viewModel.IsBusy.ShouldBeFalse();
        viewModel.StatusMessage.ShouldBe("Directory is required.");
    }

    [Fact]
    public void Constructor_InitializesAvailableOperations()
    {
        viewModel.AvailableOperations.ShouldNotBeNull();
        viewModel.AvailableOperations.ShouldContain(ArchiveOperation.DecompressArchives);
        viewModel.AvailableOperations.ShouldContain(ArchiveOperation.CompressFolder);
        viewModel.AvailableOperations.ShouldContain(ArchiveOperation.ExtractImages);
    }

    [Fact]
    public void Constructor_SetsDefaultSelectedOperation()
    {
        viewModel.SelectedOperation.ShouldBe(ArchiveOperation.DecompressArchives);
    }

    [Fact]
    public void ChangeTitle_UpdatesTitle()
    {
        viewModel.ChangeTitleCommand.Execute(null);

        viewModel.Title.ShouldBe("Stl Organizer - Updated");
    }

    [Fact]
    public async Task ExecuteOperationAsync_WithEmptyDirectory_SetsErrorMessage()
    {
        viewModel.SelectedDirectory = string.Empty;

        await viewModel.ExecuteOperationCommand.ExecuteAsync(null);

        viewModel.StatusMessage.ShouldBe("Directory is required.");
        viewModel.IsBusy.ShouldBeFalse();
    }

    [Fact]
    public async Task ExecuteOperationAsync_WithWhitespaceDirectory_SetsErrorMessage()
    {
        viewModel.SelectedDirectory = "   ";

        await viewModel.ExecuteOperationCommand.ExecuteAsync(null);

        viewModel.StatusMessage.ShouldBe("Directory is required.");
        viewModel.IsBusy.ShouldBeFalse();
    }

    [Fact]
    public async Task ExecuteOperationAsync_WithValidDirectory_CallsOperationSelector()
    {
        const string directory = @"C:\TestDir";
        const string expectedResult = "Operation completed successfully";
        viewModel.SelectedDirectory = directory;
        viewModel.SelectedOperation = ArchiveOperation.ExtractImages;

        A.CallTo(() =>
                archiveOperationSelector.ExecuteOperationAsync(
                    ArchiveOperation.ExtractImages,
                    directory,
                    A<IProgress<OrganizerProgress>>._,
                    A<CancellationToken>._))
            .Returns(Task.FromResult(expectedResult));

        await viewModel.ExecuteOperationCommand.ExecuteAsync(null);

        A.CallTo(() =>
                archiveOperationSelector.ExecuteOperationAsync(
                    ArchiveOperation.ExtractImages,
                    directory,
                    A<IProgress<OrganizerProgress>>._,
                    A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        viewModel.StatusMessage.ShouldBe(expectedResult);
    }

    [Fact]
    public async Task ExecuteOperationAsync_SetsIsExecutingDuringOperation()
    {
        const string directory = @"C:\TestDir";
        viewModel.SelectedDirectory = directory;
        var executionStarted = false;

        A.CallTo(() =>
                archiveOperationSelector.ExecuteOperationAsync(
                    A<ArchiveOperation>._,
                    A<string>._,
                    A<IProgress<OrganizerProgress>>._,
                    A<CancellationToken>._))
            .ReturnsLazily(() =>
            {
                executionStarted = viewModel.IsBusy;
                return Task.FromResult("Success");
            });

        await viewModel.ExecuteOperationCommand.ExecuteAsync(null);

        executionStarted.ShouldBeTrue();
    }

    [Fact]
    public async Task ExecuteOperationAsync_UsesCancellationTokenSourceFromProvider()
    {
        const string directory = @"C:\TestDir";
        viewModel.SelectedDirectory = directory;
        var cts = new CancellationTokenSource();
        var token = cts.Token;
        A.CallTo(() => cancellationTokenSourceProvider.Create()).Returns(cts);

        await viewModel.ExecuteOperationCommand.ExecuteAsync(null);

        A.CallTo(() => cancellationTokenSourceProvider.Create()).MustHaveHappenedOnceExactly();
        A.CallTo(() => archiveOperationSelector.ExecuteOperationAsync(
                A<ArchiveOperation>._,
                A<string>._,
                A<IProgress<OrganizerProgress>>._,
                token))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task ExecuteOperationAsync_SetsStatusMessageDuringOperation()
    {
        const string directory = @"C:\TestDir";
        viewModel.SelectedDirectory = directory;
        viewModel.SelectedOperation = ArchiveOperation.CompressFolder;

        A.CallTo(() =>
                archiveOperationSelector.ExecuteOperationAsync(
                    A<ArchiveOperation>._,
                    A<string>._,
                    A<IProgress<OrganizerProgress>>._,
                    A<CancellationToken>._))
            .Returns(Task.FromResult("Done"));

        await viewModel.ExecuteOperationCommand.ExecuteAsync(null);

        viewModel.StatusMessage.ShouldBe("Done");
    }

    [Fact]
    public async Task ExecuteOperationAsync_WithException_SetsErrorMessage()
    {
        const string directory = @"C:\TestDir";
        const string exceptionMessage = "Test exception";
        viewModel.SelectedDirectory = directory;

        A.CallTo(() =>
                archiveOperationSelector.ExecuteOperationAsync(
                    A<ArchiveOperation>._,
                    A<string>._,
                    A<IProgress<OrganizerProgress>>._,
                    A<CancellationToken>._))
            .Throws(new InvalidOperationException(exceptionMessage));

        await viewModel.ExecuteOperationCommand.ExecuteAsync(null);

        viewModel.StatusMessage.ShouldBe($"Error: {exceptionMessage}");
        viewModel.IsBusy.ShouldBeFalse();
    }

    [Fact]
    public async Task ExecuteOperationAsync_WithException_ResetsIsExecuting()
    {
        const string directory = @"C:\TestDir";
        viewModel.SelectedDirectory = directory;

        A.CallTo(() =>
                archiveOperationSelector.ExecuteOperationAsync(
                    A<ArchiveOperation>._,
                    A<string>._,
                    A<IProgress<OrganizerProgress>>._,
                    A<CancellationToken>._))
            .Throws(new Exception("Test error"));

        await viewModel.ExecuteOperationCommand.ExecuteAsync(null);

        viewModel.IsBusy.ShouldBeFalse();
    }

    [Fact]
    public async Task ExecuteOperationAsync_WithDecompressArchives_PassesCorrectType()
    {
        await ExecuteOperationAsync(ArchiveOperation.DecompressArchives);
    }

    [Fact]
    public async Task ExecuteOperationAsync_WithCompressFolder_PassesCorrectType()
    {
        await ExecuteOperationAsync(ArchiveOperation.CompressFolder);
    }

    [Fact]
    public async Task ExecuteOperationAsync_WithExtractImages_PassesCorrectType()
    {
        await ExecuteOperationAsync(ArchiveOperation.ExtractImages);
    }

    private async Task ExecuteOperationAsync(ArchiveOperation operation)
    {
        const string directory = @"C:\TestDir";
        viewModel.SelectedDirectory = directory;
        viewModel.SelectedOperation = ArchiveOperation.ExtractImages;
        viewModel.SelectedOperation = operation;

        await viewModel.ExecuteOperationCommand.ExecuteAsync(null);

        A.CallTo(() =>
                archiveOperationSelector.ExecuteOperationAsync(
                    A<ArchiveOperation>._,
                    A<string>._,
                    A<IProgress<OrganizerProgress>>._,
                    A<CancellationToken>._))
            .MustHaveHappened();
    }
}
