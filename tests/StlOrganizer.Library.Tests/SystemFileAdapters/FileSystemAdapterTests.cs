using FakeItEasy;
using Shouldly;
using StlOrganizer.Library.SystemFileAdapters;

namespace StlOrganizer.Library.Tests.SystemFileAdapters;

public class FileSystemAdapterTests
{
    [Theory]
    [InlineData(@"C:\MyFolder\File.zip", true, "MyFolder")]
    [InlineData(@"C:\MyFolder\File.txt", true, "MyFolder")]
    [InlineData(@"C:\MyFolder\target", false, "target")]
    public void GetFolderName_WhenGivenPath_ReturnsCorrectResult(string path, bool exists, string expected)
    {
        var fileSystem = A.Fake<IFileOperations>();
        A.CallTo(() => fileSystem.FileExists(path)).Returns(exists);
        var sut = new FileSystemAdapter(fileSystem);

        var output = sut.GetFolderName(path);
        
        output.ShouldBe(expected);
    }

    [Fact]
    public void GetFolderName_WhenThePathIsEmpty_ShouldThrowAnException()
    {
        var sut = new FileSystemAdapter(A.Fake<IFileOperations>());
        
        Should.Throw<ArgumentException>(() => sut.GetFolderName(string.Empty));
    }

    // [Theory]
    // [InlineData(@"C:\myfolder\target", @"C:\myfolder\target")]
    // [InlineData(@"C:\myfolder\target\file.zip", @"C:\myfolder\target")]
    public void GetDirectoryName_WhenGivenAPath_ReturnsPath(string source, string expected)
    {
        var sut = new FileSystemAdapter(A.Fake<IFileOperations>());

        sut.GetParentDirectory(source).ShouldBe(expected);
    }
}