using FakeItEasy;
using Shouldly;
using StlOrganizer.Gui.ViewModels;
using StlOrganizer.Library;

namespace StlOrganizer.Gui.Tests.ViewModels;

public class MainViewModelTests
{
    private readonly IOperationSelector operationSelector;
    private readonly MainViewModel viewModel;

    public MainViewModelTests()
    {
        operationSelector = A.Fake<IOperationSelector>();
        viewModel = new MainViewModel(operationSelector);
    }

    [Fact]
    public void Constructor_InitializesProperties()
    {
        viewModel.Title.ShouldBe("Stl Organizer");
        viewModel.TextFieldValue.ShouldBe(string.Empty);
        viewModel.SelectedDirectory.ShouldBe(string.Empty);
        viewModel.IsExecuting.ShouldBeFalse();
        viewModel.StatusMessage.ShouldBe(string.Empty);
    }

    [Fact]
    public void Constructor_InitializesAvailableOperations()
    {
        viewModel.AvailableOperations.ShouldNotBeNull();
        viewModel.AvailableOperations.Count.ShouldBe(3);
        viewModel.AvailableOperations.ShouldContain(OperationType.FileDecompressor);
        viewModel.AvailableOperations.ShouldContain(OperationType.FolderCompressor);
        viewModel.AvailableOperations.ShouldContain(OperationType.ImageOrganizer);
    }

    [Fact]
    public void Constructor_SetsDefaultSelectedOperation()
    {
        viewModel.SelectedOperation.ShouldBe(OperationType.FileDecompressor);
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

        viewModel.StatusMessage.ShouldBe("Please select a directory first.");
        viewModel.IsExecuting.ShouldBeFalse();
    }

    [Fact]
    public async Task ExecuteOperationAsync_WithWhitespaceDirectory_SetsErrorMessage()
    {
        viewModel.SelectedDirectory = "   ";

        await viewModel.ExecuteOperationCommand.ExecuteAsync(null);

        viewModel.StatusMessage.ShouldBe("Please select a directory first.");
        viewModel.IsExecuting.ShouldBeFalse();
    }

    [Fact]
    public async Task ExecuteOperationAsync_WithValidDirectory_CallsOperationSelector()
    {
        const string directory = @"C:\TestDir";
        const string expectedResult = "Operation completed successfully";
        viewModel.SelectedDirectory = directory;
        viewModel.SelectedOperation = OperationType.ImageOrganizer;

        A.CallTo(() => operationSelector.ExecuteOperationAsync(OperationType.ImageOrganizer, directory))
            .Returns(Task.FromResult(expectedResult));

        await viewModel.ExecuteOperationCommand.ExecuteAsync(null);

        A.CallTo(() => operationSelector.ExecuteOperationAsync(OperationType.ImageOrganizer, directory))
            .MustHaveHappenedOnceExactly();
        viewModel.StatusMessage.ShouldBe(expectedResult);
    }

    [Fact]
    public async Task ExecuteOperationAsync_SetsIsExecutingDuringOperation()
    {
        const string directory = @"C:\TestDir";
        viewModel.SelectedDirectory = directory;
        var executionStarted = false;

        A.CallTo(() => operationSelector.ExecuteOperationAsync(A<OperationType>._, A<string>._))
            .ReturnsLazily(() =>
            {
                executionStarted = viewModel.IsExecuting;
                return Task.FromResult("Success");
            });

        await viewModel.ExecuteOperationCommand.ExecuteAsync(null);

        executionStarted.ShouldBeTrue();
        viewModel.IsExecuting.ShouldBeFalse();
    }

    [Fact]
    public async Task ExecuteOperationAsync_SetsStatusMessageDuringOperation()
    {
        const string directory = @"C:\TestDir";
        viewModel.SelectedDirectory = directory;
        viewModel.SelectedOperation = OperationType.FolderCompressor;

        A.CallTo(() => operationSelector.ExecuteOperationAsync(A<OperationType>._, A<string>._))
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

        A.CallTo(() => operationSelector.ExecuteOperationAsync(A<OperationType>._, A<string>._))
            .Throws(new InvalidOperationException(exceptionMessage));

        await viewModel.ExecuteOperationCommand.ExecuteAsync(null);

        viewModel.StatusMessage.ShouldBe($"Error: {exceptionMessage}");
        viewModel.IsExecuting.ShouldBeFalse();
    }

    [Fact]
    public async Task ExecuteOperationAsync_WithException_ResetsIsExecuting()
    {
        const string directory = @"C:\TestDir";
        viewModel.SelectedDirectory = directory;

        A.CallTo(() => operationSelector.ExecuteOperationAsync(A<OperationType>._, A<string>._))
            .Throws(new Exception("Test error"));

        await viewModel.ExecuteOperationCommand.ExecuteAsync(null);

        viewModel.IsExecuting.ShouldBeFalse();
    }

    [Fact]
    public void SelectedOperation_CanBeChanged()
    {
        viewModel.SelectedOperation = OperationType.FolderCompressor;
        viewModel.SelectedOperation.ShouldBe(OperationType.FolderCompressor);

        viewModel.SelectedOperation = OperationType.ImageOrganizer;
        viewModel.SelectedOperation.ShouldBe(OperationType.ImageOrganizer);
    }

    [Fact]
    public void SelectedDirectory_CanBeChanged()
    {
        const string directory = @"C:\TestDirectory";
        
        viewModel.SelectedDirectory = directory;
        
        viewModel.SelectedDirectory.ShouldBe(directory);
    }

    [Fact]
    public void TextFieldValue_CanBeChanged()
    {
        const string value = "Test Value";
        
        viewModel.TextFieldValue = value;
        
        viewModel.TextFieldValue.ShouldBe(value);
    }

    [Fact]
    public async Task ExecuteOperationAsync_WithDifferentOperationTypes_PassesCorrectType()
    {
        const string directory = @"C:\TestDir";
        viewModel.SelectedDirectory = directory;

        // Test FileDecompressor
        viewModel.SelectedOperation = OperationType.FileDecompressor;
        await viewModel.ExecuteOperationCommand.ExecuteAsync(null);
        A.CallTo(() => operationSelector.ExecuteOperationAsync(OperationType.FileDecompressor, directory))
            .MustHaveHappened();

        // Test FolderCompressor
        viewModel.SelectedOperation = OperationType.FolderCompressor;
        await viewModel.ExecuteOperationCommand.ExecuteAsync(null);
        A.CallTo(() => operationSelector.ExecuteOperationAsync(OperationType.FolderCompressor, directory))
            .MustHaveHappened();

        // Test ImageOrganizer
        viewModel.SelectedOperation = OperationType.ImageOrganizer;
        await viewModel.ExecuteOperationCommand.ExecuteAsync(null);
        A.CallTo(() => operationSelector.ExecuteOperationAsync(OperationType.ImageOrganizer, directory))
            .MustHaveHappened();
    }
}
