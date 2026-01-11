using FakeItEasy;
using Shouldly;
using StlOrganizer.Library.Decompression;
using StlOrganizer.Library.SystemFileAdapters;

namespace StlOrganizer.Library.Tests.Decompression;

public class FolderFlattenerTests
{
    private readonly IDirectoryService directoryService;
    private readonly IFolderFlattener flattener;

    public FolderFlattenerTests()
    {
        directoryService = A.Fake<IDirectoryService>();
        flattener = new FolderFlattener(directoryService);
    }

    [Fact]
    public void RemoveNestedFolders_WhenDirectoryDoesNotExist_ThrowsDirectoryNotFoundException()
    {
        const string path = @"C:\NonExistent";
        A.CallTo(() => directoryService.Exists(path)).Returns(false);

        Should.Throw<DirectoryNotFoundException>(() => flattener.RemoveNestedFolders(path));
    }

    [Fact]
    public void RemoveNestedFolders_WhenDirectoryExists_ChecksExistence()
    {
        const string path = @"C:\TestDir";
        A.CallTo(() => directoryService.Exists(path)).Returns(true);
        A.CallTo(() => directoryService.GetDirectories(path)).Returns(Array.Empty<string>());

        flattener.RemoveNestedFolders(path);

        A.CallTo(() => directoryService.Exists(path)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void RemoveNestedFolders_WhenNoSubdirectories_DoesNothing()
    {
        const string path = @"C:\TestDir";
        A.CallTo(() => directoryService.Exists(path)).Returns(true);
        A.CallTo(() => directoryService.GetDirectories(path)).Returns(Array.Empty<string>());

        flattener.RemoveNestedFolders(path);

        A.CallTo(() => directoryService.Move(A<string>._, A<string>._)).MustNotHaveHappened();
        A.CallTo(() => directoryService.Delete(A<string>._, A<bool>._)).MustNotHaveHappened();
    }

    [Fact]
    public void RemoveNestedFolders_WhenSubdirectoryNameMatches_FlattensFolder()
    {
        const string parentPath = @"C:\TestDir";
        const string childPath = @"C:\TestDir\TestDir";

        A.CallTo(() => directoryService.Exists(parentPath)).Returns(true);
        A.CallTo(() => directoryService.GetDirectories(parentPath)).Returns([childPath]);
        A.CallTo(() => directoryService.GetDirectories(childPath)).Returns([]);
        A.CallTo(() => directoryService.GetDirectoryName(parentPath)).Returns("TestDir");
        A.CallTo(() => directoryService.GetDirectoryName(childPath)).Returns("TestDir");

        flattener.RemoveNestedFolders(parentPath);

        A.CallTo(() => directoryService.Move(childPath, parentPath)).MustHaveHappenedOnceExactly();
        A.CallTo(() => directoryService.Delete(childPath, false)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void RemoveNestedFolders_WhenSubdirectoryNameDoesNotMatch_DoesNotFlatten()
    {
        const string parentPath = @"C:\TestDir";
        const string childPath = @"C:\TestDir\OtherDir";

        A.CallTo(() => directoryService.Exists(parentPath)).Returns(true);
        A.CallTo(() => directoryService.GetDirectories(parentPath)).Returns([childPath]);
        A.CallTo(() => directoryService.GetDirectories(childPath)).Returns([]);
        A.CallTo(() => directoryService.GetDirectoryName(parentPath)).Returns("TestDir");
        A.CallTo(() => directoryService.GetDirectoryName(childPath)).Returns("OtherDir");

        flattener.RemoveNestedFolders(parentPath);

        A.CallTo(() => directoryService.Move(A<string>._, A<string>._)).MustNotHaveHappened();
        A.CallTo(() => directoryService.Delete(A<string>._, A<bool>._)).MustNotHaveHappened();
    }

    [Fact]
    public void RemoveNestedFolders_IsCaseInsensitive()
    {
        const string parentPath = @"C:\TestDir";
        const string childPath = @"C:\TestDir\testdir";

        A.CallTo(() => directoryService.Exists(parentPath)).Returns(true);
        A.CallTo(() => directoryService.GetDirectories(parentPath)).Returns([childPath]);
        A.CallTo(() => directoryService.GetDirectories(childPath)).Returns([]);
        A.CallTo(() => directoryService.GetDirectoryName(parentPath)).Returns("TestDir");
        A.CallTo(() => directoryService.GetDirectoryName(childPath)).Returns("testdir");

        flattener.RemoveNestedFolders(parentPath);

        A.CallTo(() => directoryService.Move(childPath, parentPath)).MustHaveHappenedOnceExactly();
        A.CallTo(() => directoryService.Delete(childPath, false)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void RemoveNestedFolders_WithMultipleSubdirectories_ProcessesAll()
    {
        const string parentPath = @"C:\Root";
        const string matchingChild = @"C:\Root\Root";
        const string nonMatchingChild = @"C:\Root\Other";

        A.CallTo(() => directoryService.Exists(parentPath)).Returns(true);
        A.CallTo(() => directoryService.GetDirectories(parentPath))
            .Returns([matchingChild, nonMatchingChild]);
        A.CallTo(() => directoryService.GetDirectories(matchingChild)).Returns([]);
        A.CallTo(() => directoryService.GetDirectories(nonMatchingChild)).Returns([]);
        A.CallTo(() => directoryService.GetDirectoryName(parentPath)).Returns("Root");
        A.CallTo(() => directoryService.GetDirectoryName(matchingChild)).Returns("Root");
        A.CallTo(() => directoryService.GetDirectoryName(nonMatchingChild)).Returns("Other");

        flattener.RemoveNestedFolders(parentPath);

        A.CallTo(() => directoryService.Move(matchingChild, parentPath)).MustHaveHappenedOnceExactly();
        A.CallTo(() => directoryService.Delete(matchingChild, false)).MustHaveHappenedOnceExactly();
        A.CallTo(() => directoryService.Move(nonMatchingChild, A<string>._)).MustNotHaveHappened();
    }

    [Fact]
    public void RemoveNestedFolders_ProcessesNestedDirectoriesRecursively()
    {
        const string rootPath = @"C:\Root";
        const string level1Path = @"C:\Root\SubDir";
        const string level2Path = @"C:\Root\SubDir\SubDir";

        A.CallTo(() => directoryService.Exists(rootPath)).Returns(true);
        A.CallTo(() => directoryService.GetDirectories(rootPath)).Returns([level1Path]);
        A.CallTo(() => directoryService.GetDirectories(level1Path)).Returns([level2Path]);
        A.CallTo(() => directoryService.GetDirectories(level2Path)).Returns([]);
        A.CallTo(() => directoryService.GetDirectoryName(rootPath)).Returns("Root");
        A.CallTo(() => directoryService.GetDirectoryName(level1Path)).Returns("SubDir");
        A.CallTo(() => directoryService.GetDirectoryName(level2Path)).Returns("SubDir");

        flattener.RemoveNestedFolders(rootPath);

        A.CallTo(() => directoryService.Move(level2Path, level1Path)).MustHaveHappenedOnceExactly();
        A.CallTo(() => directoryService.Delete(level2Path, false)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void RemoveNestedFolders_WithDeepNestedMatchingFolders_FlattensAll()
    {
        const string rootPath = @"C:\Archive";
        const string level1Path = @"C:\Archive\Archive";
        const string level2Path = @"C:\Archive\Archive\Archive";

        A.CallTo(() => directoryService.Exists(rootPath)).Returns(true);
        A.CallTo(() => directoryService.GetDirectories(rootPath)).Returns([level1Path]);
        A.CallTo(() => directoryService.GetDirectories(level1Path))
            .ReturnsNextFromSequence([level2Path], Array.Empty<string>());
        A.CallTo(() => directoryService.GetDirectories(level2Path)).Returns([]);
        A.CallTo(() => directoryService.GetDirectoryName(rootPath)).Returns("Archive");
        A.CallTo(() => directoryService.GetDirectoryName(level1Path)).Returns("Archive");
        A.CallTo(() => directoryService.GetDirectoryName(level2Path)).Returns("Archive");

        flattener.RemoveNestedFolders(rootPath);

        A.CallTo(() => directoryService.Move(level2Path, level1Path)).MustHaveHappened();
        A.CallTo(() => directoryService.Move(level1Path, rootPath)).MustHaveHappened();
    }

    [Fact]
    public void RemoveNestedFolders_DeletesWithNonRecursiveOption()
    {
        const string parentPath = @"C:\TestDir";
        const string childPath = @"C:\TestDir\TestDir";

        A.CallTo(() => directoryService.Exists(parentPath)).Returns(true);
        A.CallTo(() => directoryService.GetDirectories(parentPath)).Returns([childPath]);
        A.CallTo(() => directoryService.GetDirectories(childPath)).Returns([]);
        A.CallTo(() => directoryService.GetDirectoryName(parentPath)).Returns("TestDir");
        A.CallTo(() => directoryService.GetDirectoryName(childPath)).Returns("TestDir");

        flattener.RemoveNestedFolders(parentPath);

        A.CallTo(() => directoryService.Delete(childPath, false)).MustHaveHappenedOnceExactly();
        A.CallTo(() => directoryService.Delete(childPath, true)).MustNotHaveHappened();
    }
}
