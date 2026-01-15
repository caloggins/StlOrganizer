using FakeItEasy;
using Shouldly;
using StlOrganizer.Library.Decompression;
using StlOrganizer.Library.OperationSelection;
using StlOrganizer.Library.SystemAdapters.FileSystem;

namespace StlOrganizer.Library.Tests.Decompression;

public class DecompressionWorkflowTests
{
    private readonly IFolderScanner fileDecompressor;
    private readonly IFolderFlattener folderFlattener;
    private readonly IFileOperations fileOperations;
    private readonly DecompressionWorkflow workflow;

    public DecompressionWorkflowTests()
    {
        fileDecompressor = A.Fake<IFolderScanner>();
        folderFlattener = A.Fake<IFolderFlattener>();
        fileOperations = A.Fake<IFileOperations>();
        workflow = new DecompressionWorkflow(fileDecompressor, folderFlattener, fileOperations);
    }
    
    [Fact]
    public async Task Execute_DirectoryDoesNotExist_ShouldThrowException()
    {
        const string path = @"C:\test";
        A.CallTo(() => fileOperations.DirectoryExists(path)).Returns(false);
        
        await workflow.Execute(
                path,
                new Progress<OrganizerProgress>())
            .ShouldThrowAsync<DirectoryNotFoundException>()
            .ContinueWith(t => t.Result.Message.ShouldBe($"{path}"));
    }
    
    [Fact]
    public async Task Execute_DirectoryExists_ExecutesWorkflow()
    {
        var organizerProgress = A.Fake<IProgress<OrganizerProgress>>();
        const string path = @"C:\test";
        A.CallTo(() => fileOperations.DirectoryExists(path)).Returns(true);
        
        await workflow.Execute(path, organizerProgress, CancellationToken.None);
        
        A.CallTo(() => fileDecompressor.FindAndDecompress(
                path,
                organizerProgress,
                A<CancellationToken>._)).MustHaveHappened()
            .Then(A.CallTo(() => 
                folderFlattener.RemoveNestedFolders(path, A<CancellationToken>._))
                .MustHaveHappened());
    }
}
