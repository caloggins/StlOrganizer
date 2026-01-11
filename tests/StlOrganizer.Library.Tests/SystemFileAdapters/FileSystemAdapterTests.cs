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
    public void GetDirectoryName_WhenGivenPath_ReturnsCorrectResult(string path, bool exists, string expected)
    {
        var fileSystem = A.Fake<IFileOperations>();
        A.CallTo(() => fileSystem.FileExists(path)).Returns(exists);
        var sut = new FileSystemAdapter(fileSystem);

        var output = sut.GetDirectoryName(path);
        
        output.ShouldBe(expected);
    }
}