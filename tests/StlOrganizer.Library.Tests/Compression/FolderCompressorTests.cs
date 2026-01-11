using System.IO.Compression;
using FakeItEasy;
using Serilog;
using Shouldly;
using StlOrganizer.Library.Compression;
using StlOrganizer.Library.SystemFileAdapters;

namespace StlOrganizer.Library.Tests.Compression;

public class FolderCompressorTests
{
    private readonly IFileSystem fileSystem;
    private readonly IFileOperations fileOperations;
    private readonly IZipArchiveFactory zipArchiveFactory;
    private readonly ILogger logger;
    private readonly FolderCompressor compressor;

    public FolderCompressorTests()
    {
        fileSystem = A.Fake<IFileSystem>();
        fileOperations = A.Fake<IFileOperations>();
        zipArchiveFactory = A.Fake<IZipArchiveFactory>();
        logger = A.Fake<ILogger>();
        compressor = new FolderCompressor(fileSystem, fileOperations, zipArchiveFactory, logger);
    }
    
    [Fact]
    public void CompressFolder_WhenDirectoryDoesNotExist_ThrowsDirectoryNotFoundException()
    {
        const string folderPath = @"C:\NonExistent";
        A.CallTo(() => fileSystem.DirectoryExists(folderPath)).Returns(false);

        Should.Throw<DirectoryNotFoundException>(() => compressor.CompressFolder(folderPath));
    }

    [Fact]
    public void CompressFolder_WhenDirectoryExists_ChecksDirectoryExists()
    {
        const string folderPath = @"C:\TestDir";
        const string outputPath = @"C:\TestDir.zip";
        var archive = A.Fake<IZipArchive>();

        A.CallTo(() => fileSystem.DirectoryExists(folderPath)).Returns(true);
        A.CallTo(() => fileSystem.GetFolderName(folderPath)).Returns("TestDir");
        A.CallTo(() => fileSystem.GetParentDirectory(folderPath)).Returns(@"C:\");
        A.CallTo(() => fileSystem.CombinePaths(@"C:\", "TestDir.zip")).Returns(outputPath);
        A.CallTo(() => fileOperations.FileExists(outputPath)).Returns(false);
        A.CallTo(() => zipArchiveFactory.Open(outputPath, ZipArchiveMode.Create)).Returns(archive);
        A.CallTo(() => fileSystem.GetFiles(folderPath, "*.*", SearchOption.TopDirectoryOnly)).Returns([]);
        A.CallTo(() => fileSystem.GetDirectories(folderPath)).Returns([]);

        compressor.CompressFolder(folderPath);

        A.CallTo(() => fileSystem.DirectoryExists(folderPath)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void CompressFolder_WhenTheFolderIsASubFolder_ItUsesTheSubfolder()
    {
        const string folderPath = @"C:\MyFolder\Target";
        const string expectedOutput = @"C:\MyFolder\Target.zip";
        var archive = A.Fake<IZipArchive>();

        A.CallTo(() => fileSystem.DirectoryExists(folderPath)).Returns(true);
        A.CallTo(() => fileSystem.GetFolderName(folderPath)).Returns("MyFolder");
        A.CallTo(() => fileSystem.GetParentDirectory(folderPath)).Returns(@"C:\");
        A.CallTo(() => fileSystem.CombinePaths(@"C:\", "MyFolder.zip")).Returns(expectedOutput);
        A.CallTo(() => fileOperations.FileExists(expectedOutput)).Returns(false);
        A.CallTo(() => zipArchiveFactory.Open(expectedOutput, ZipArchiveMode.Create)).Returns(archive);
        A.CallTo(() => fileSystem.GetFiles(folderPath, "*.*", SearchOption.TopDirectoryOnly)).Returns([]);
        A.CallTo(() => fileSystem.GetDirectories(folderPath)).Returns([]);

        var result = compressor.CompressFolder(folderPath);

        result.ShouldBe(expectedOutput);
    }

    [Fact]
    public void CompressFolder_WhenNoOutputPathProvided_GeneratesDefaultPath()
    {
        const string folderPath = @"C:\MyFolder";
        const string expectedOutput = @"C:\MyFolder.zip";
        var archive = A.Fake<IZipArchive>();

        A.CallTo(() => fileSystem.DirectoryExists(folderPath)).Returns(true);
        A.CallTo(() => fileSystem.GetFolderName(folderPath)).Returns("MyFolder");
        A.CallTo(() => fileSystem.GetParentDirectory(folderPath)).Returns(@"C:\");
        A.CallTo(() => fileSystem.CombinePaths(@"C:\", "MyFolder.zip")).Returns(expectedOutput);
        A.CallTo(() => fileOperations.FileExists(expectedOutput)).Returns(false);
        A.CallTo(() => zipArchiveFactory.Open(expectedOutput, ZipArchiveMode.Create)).Returns(archive);
        A.CallTo(() => fileSystem.GetFiles(folderPath, "*.*", SearchOption.TopDirectoryOnly)).Returns([]);
        A.CallTo(() => fileSystem.GetDirectories(folderPath)).Returns([]);

        var result = compressor.CompressFolder(folderPath);

        result.ShouldBe(expectedOutput);
    }
    
    

    [Fact]
    public void CompressFolder_WhenOutputPathProvided_UsesProvidedPath()
    {
        const string folderPath = @"C:\MyFolder";
        const string customOutput = @"D:\Backup\archive.zip";
        var archive = A.Fake<IZipArchive>();

        A.CallTo(() => fileSystem.DirectoryExists(folderPath)).Returns(true);
        A.CallTo(() => fileOperations.FileExists(customOutput)).Returns(false);
        A.CallTo(() => zipArchiveFactory.Open(customOutput, ZipArchiveMode.Create)).Returns(archive);
        A.CallTo(() => fileSystem.GetFiles(folderPath, "*.*", SearchOption.TopDirectoryOnly)).Returns([]);
        A.CallTo(() => fileSystem.GetDirectories(folderPath)).Returns([]);

        var result = compressor.CompressFolder(folderPath, customOutput);

        result.ShouldBe(customOutput);
    }

    [Fact]
    public void CompressFolder_WhenOutputFileExists_DeletesExistingFile()
    {
        const string folderPath = @"C:\MyFolder";
        const string outputPath = @"C:\MyFolder.zip";
        var archive = A.Fake<IZipArchive>();

        A.CallTo(() => fileSystem.DirectoryExists(folderPath)).Returns(true);
        A.CallTo(() => fileSystem.GetFolderName(folderPath)).Returns("MyFolder");
        A.CallTo(() => fileSystem.GetParentDirectory(folderPath)).Returns(@"C:\");
        A.CallTo(() => fileSystem.CombinePaths(@"C:\", "MyFolder.zip")).Returns(outputPath);
        A.CallTo(() => fileOperations.FileExists(outputPath)).Returns(true);
        A.CallTo(() => zipArchiveFactory.Open(outputPath, ZipArchiveMode.Create)).Returns(archive);
        A.CallTo(() => fileSystem.GetFiles(folderPath, "*.*", SearchOption.TopDirectoryOnly)).Returns([]);
        A.CallTo(() => fileSystem.GetDirectories(folderPath)).Returns([]);

        compressor.CompressFolder(folderPath);

        A.CallTo(() => fileOperations.DeleteFile(outputPath)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void CompressFolder_CreatesZipArchiveInCreateMode()
    {
        const string folderPath = @"C:\MyFolder";
        const string outputPath = @"C:\MyFolder.zip";
        var archive = A.Fake<IZipArchive>();

        A.CallTo(() => fileSystem.DirectoryExists(folderPath)).Returns(true);
        A.CallTo(() => fileSystem.GetFolderName(folderPath)).Returns("MyFolder");
        A.CallTo(() => fileSystem.GetParentDirectory(folderPath)).Returns(@"C:\");
        A.CallTo(() => fileSystem.CombinePaths(@"C:\", "MyFolder.zip")).Returns(outputPath);
        A.CallTo(() => fileOperations.FileExists(outputPath)).Returns(false);
        A.CallTo(() => zipArchiveFactory.Open(outputPath, ZipArchiveMode.Create)).Returns(archive);
        A.CallTo(() => fileSystem.GetFiles(folderPath, "*.*", SearchOption.TopDirectoryOnly)).Returns([]);
        A.CallTo(() => fileSystem.GetDirectories(folderPath)).Returns([]);

        compressor.CompressFolder(folderPath);

        A.CallTo(() => zipArchiveFactory.Open(outputPath, ZipArchiveMode.Create)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void CompressFolder_AddsFilesToArchive()
    {
        const string folderPath = @"C:\MyFolder";
        const string outputPath = @"C:\MyFolder.zip";
        const string file1 = @"C:\MyFolder\file1.txt";
        const string file2 = @"C:\MyFolder\file2.doc";
        var archive = A.Fake<IZipArchive>();

        A.CallTo(() => fileSystem.DirectoryExists(folderPath)).Returns(true);
        A.CallTo(() => fileSystem.GetFolderName(folderPath)).Returns("MyFolder");
        A.CallTo(() => fileSystem.GetParentDirectory(folderPath)).Returns(@"C:\");
        A.CallTo(() => fileSystem.CombinePaths(@"C:\", "MyFolder.zip")).Returns(outputPath);
        A.CallTo(() => fileOperations.FileExists(outputPath)).Returns(false);
        A.CallTo(() => zipArchiveFactory.Open(outputPath, ZipArchiveMode.Create)).Returns(archive);
        A.CallTo(() => fileSystem.GetFiles(folderPath, "*.*", SearchOption.TopDirectoryOnly))
            .Returns([file1, file2]);
        A.CallTo(() => fileOperations.GetFileName(file1)).Returns("file1.txt");
        A.CallTo(() => fileOperations.GetFileName(file2)).Returns("file2.doc");
        A.CallTo(() => fileSystem.GetDirectories(folderPath)).Returns([]);

        compressor.CompressFolder(folderPath);

        A.CallTo(() => archive.CreateEntryFromFile(file1, "file1.txt", CompressionLevel.Optimal))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => archive.CreateEntryFromFile(file2, "file2.doc", CompressionLevel.Optimal))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void CompressFolder_ProcessesSubdirectoriesRecursively()
    {
        const string folderPath = @"C:\MyFolder";
        const string outputPath = @"C:\MyFolder.zip";
        const string subDir = @"C:\MyFolder\SubDir";
        const string file = @"C:\MyFolder\SubDir\file.txt";
        var archive = A.Fake<IZipArchive>();

        A.CallTo(() => fileSystem.DirectoryExists(folderPath)).Returns(true);
        A.CallTo(() => fileSystem.GetFolderName(folderPath)).Returns("MyFolder");
        A.CallTo(() => fileSystem.GetParentDirectory(folderPath)).Returns(@"C:\");
        A.CallTo(() => fileSystem.CombinePaths(@"C:\", "MyFolder.zip")).Returns(outputPath);
        A.CallTo(() => fileOperations.FileExists(outputPath)).Returns(false);
        A.CallTo(() => zipArchiveFactory.Open(outputPath, ZipArchiveMode.Create)).Returns(archive);
        A.CallTo(() => fileSystem.GetFiles(folderPath, "*.*", SearchOption.TopDirectoryOnly)).Returns([]);
        A.CallTo(() => fileSystem.GetDirectories(folderPath)).Returns([subDir]);
        A.CallTo(() => fileSystem.GetFolderName(subDir)).Returns("SubDir");
        A.CallTo(() => fileSystem.GetFiles(subDir, "*.*", SearchOption.TopDirectoryOnly)).Returns([file]);
        A.CallTo(() => fileOperations.GetFileName(file)).Returns("file.txt");
        A.CallTo(() => fileSystem.CombinePaths("SubDir", "file.txt")).Returns(@"SubDir\file.txt");
        A.CallTo(() => fileSystem.GetDirectories(subDir)).Returns([]);

        compressor.CompressFolder(folderPath);

        A.CallTo(() => archive.CreateEntryFromFile(file, "SubDir/file.txt", CompressionLevel.Optimal))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void CompressFolder_PreservesDirectoryStructure()
    {
        const string folderPath = @"C:\Root";
        const string outputPath = @"C:\Root.zip";
        const string subDir = @"C:\Root\Folder";
        const string nestedDir = @"C:\Root\Folder\Nested";
        const string file = @"C:\Root\Folder\Nested\deep.txt";
        var archive = A.Fake<IZipArchive>();

        A.CallTo(() => fileSystem.DirectoryExists(folderPath)).Returns(true);
        A.CallTo(() => fileSystem.GetFolderName(folderPath)).Returns("Root");
        A.CallTo(() => fileSystem.GetParentDirectory(folderPath)).Returns(@"C:\");
        A.CallTo(() => fileSystem.CombinePaths(@"C:\", "Root.zip")).Returns(outputPath);
        A.CallTo(() => fileOperations.FileExists(outputPath)).Returns(false);
        A.CallTo(() => zipArchiveFactory.Open(outputPath, ZipArchiveMode.Create)).Returns(archive);

        // Root level - entryPrefix is empty string at start
        A.CallTo(() => fileSystem.GetFiles(folderPath, "*.*", SearchOption.TopDirectoryOnly)).Returns([]);
        A.CallTo(() => fileSystem.GetDirectories(folderPath)).Returns([subDir]);
        A.CallTo(() => fileSystem.GetFolderName(subDir)).Returns("Folder");
        // newEntryPrefix becomes "Folder" (no CombinePaths call since entryPrefix is empty)

        // Folder level - entryPrefix is now "Folder"
        A.CallTo(() => fileSystem.GetFiles(subDir, "*.*", SearchOption.TopDirectoryOnly)).Returns([]);
        A.CallTo(() => fileSystem.GetDirectories(subDir)).Returns([nestedDir]);
        A.CallTo(() => fileSystem.GetFolderName(nestedDir)).Returns("Nested");
        A.CallTo(() => fileSystem.CombinePaths("Folder", "Nested")).Returns(@"Folder\Nested");
        // newEntryPrefix becomes "Folder\Nested" then .Replace('\\', '/') = "Folder/Nested"

        // Nested level - entryPrefix is now "Folder/Nested"
        A.CallTo(() => fileSystem.GetFiles(nestedDir, "*.*", SearchOption.TopDirectoryOnly)).Returns([file]);
        A.CallTo(() => fileOperations.GetFileName(file)).Returns("deep.txt");
        A.CallTo(() => fileSystem.CombinePaths("Folder/Nested", "deep.txt")).Returns(@"Folder/Nested\deep.txt");
        // entryName becomes "Folder/Nested\deep.txt".Replace('\\', '/') = "Folder/Nested/deep.txt"
        A.CallTo(() => fileSystem.GetDirectories(nestedDir)).Returns([]);

        compressor.CompressFolder(folderPath);

        A.CallTo(() => archive.CreateEntryFromFile(file, "Folder/Nested/deep.txt", CompressionLevel.Optimal))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void CompressFolder_WhenFileAdditionFails_LogsErrorAndContinues()
    {
        const string folderPath = @"C:\MyFolder";
        const string outputPath = @"C:\MyFolder.zip";
        const string file1 = @"C:\MyFolder\file1.txt";
        const string file2 = @"C:\MyFolder\file2.txt";
        var archive = A.Fake<IZipArchive>();
        var exception = new IOException("File locked");

        A.CallTo(() => fileSystem.DirectoryExists(folderPath)).Returns(true);
        A.CallTo(() => fileSystem.GetFolderName(folderPath)).Returns("MyFolder");
        A.CallTo(() => fileSystem.GetParentDirectory(folderPath)).Returns(@"C:\");
        A.CallTo(() => fileSystem.CombinePaths(@"C:\", "MyFolder.zip")).Returns(outputPath);
        A.CallTo(() => fileOperations.FileExists(outputPath)).Returns(false);
        A.CallTo(() => zipArchiveFactory.Open(outputPath, ZipArchiveMode.Create)).Returns(archive);
        A.CallTo(() => fileSystem.GetFiles(folderPath, "*.*", SearchOption.TopDirectoryOnly))
            .Returns([file1, file2]);
        A.CallTo(() => fileOperations.GetFileName(file1)).Returns("file1.txt");
        A.CallTo(() => fileOperations.GetFileName(file2)).Returns("file2.txt");
        A.CallTo(() => archive.CreateEntryFromFile(file1, "file1.txt", CompressionLevel.Optimal))
            .Throws(exception);
        A.CallTo(() => fileSystem.GetDirectories(folderPath)).Returns([]);

        compressor.CompressFolder(folderPath);

        A.CallTo(() => logger.Error(exception, A<string>.That.Contains("Failed to add file"), file1))
            .MustHaveHappened();
        A.CallTo(() => archive.CreateEntryFromFile(file2, "file2.txt", CompressionLevel.Optimal))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void CompressFolder_LogsSuccessfulCreation()
    {
        const string folderPath = @"C:\MyFolder";
        const string outputPath = @"C:\MyFolder.zip";
        var archive = A.Fake<IZipArchive>();

        A.CallTo(() => fileSystem.DirectoryExists(folderPath)).Returns(true);
        A.CallTo(() => fileSystem.GetFolderName(folderPath)).Returns("MyFolder");
        A.CallTo(() => fileSystem.GetParentDirectory(folderPath)).Returns(@"C:\");
        A.CallTo(() => fileSystem.CombinePaths(@"C:\", "MyFolder.zip")).Returns(outputPath);
        A.CallTo(() => fileOperations.FileExists(outputPath)).Returns(false);
        A.CallTo(() => zipArchiveFactory.Open(outputPath, ZipArchiveMode.Create)).Returns(archive);
        A.CallTo(() => fileSystem.GetFiles(folderPath, "*.*", SearchOption.TopDirectoryOnly)).Returns([]);
        A.CallTo(() => fileSystem.GetDirectories(folderPath)).Returns([]);

        compressor.CompressFolder(folderPath);

        A.CallTo(() => logger.Information(
                A<string>.That.Contains("Created archive"),
                outputPath,
                folderPath))
            .MustHaveHappened();
    }

    [Fact]
    public void CompressFolder_DisposesArchiveAfterCompletion()
    {
        const string folderPath = @"C:\MyFolder";
        const string outputPath = @"C:\MyFolder.zip";
        var archive = A.Fake<IZipArchive>();

        A.CallTo(() => fileSystem.DirectoryExists(folderPath)).Returns(true);
        A.CallTo(() => fileSystem.GetFolderName(folderPath)).Returns("MyFolder");
        A.CallTo(() => fileSystem.GetParentDirectory(folderPath)).Returns(@"C:\");
        A.CallTo(() => fileSystem.CombinePaths(@"C:\", "MyFolder.zip")).Returns(outputPath);
        A.CallTo(() => fileOperations.FileExists(outputPath)).Returns(false);
        A.CallTo(() => zipArchiveFactory.Open(outputPath, ZipArchiveMode.Create)).Returns(archive);
        A.CallTo(() => fileSystem.GetFiles(folderPath, "*.*", SearchOption.TopDirectoryOnly)).Returns([]);
        A.CallTo(() => fileSystem.GetDirectories(folderPath)).Returns([]);

        compressor.CompressFolder(folderPath);

        A.CallTo(() => archive.Dispose()).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void CompressFolder_UsesOptimalCompressionLevel()
    {
        const string folderPath = @"C:\MyFolder";
        const string outputPath = @"C:\MyFolder.zip";
        const string file = @"C:\MyFolder\file.txt";
        var archive = A.Fake<IZipArchive>();

        A.CallTo(() => fileSystem.DirectoryExists(folderPath)).Returns(true);
        A.CallTo(() => fileSystem.GetFolderName(folderPath)).Returns("MyFolder");
        A.CallTo(() => fileSystem.GetParentDirectory(folderPath)).Returns(@"C:\");
        A.CallTo(() => fileSystem.CombinePaths(@"C:\", "MyFolder.zip")).Returns(outputPath);
        A.CallTo(() => fileOperations.FileExists(outputPath)).Returns(false);
        A.CallTo(() => zipArchiveFactory.Open(outputPath, ZipArchiveMode.Create)).Returns(archive);
        A.CallTo(() => fileSystem.GetFiles(folderPath, "*.*", SearchOption.TopDirectoryOnly)).Returns([file]);
        A.CallTo(() => fileOperations.GetFileName(file)).Returns("file.txt");
        A.CallTo(() => fileSystem.GetDirectories(folderPath)).Returns([]);

        compressor.CompressFolder(folderPath);

        A.CallTo(() => archive.CreateEntryFromFile(file, A<string>._, CompressionLevel.Optimal))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void CompressFolder_ReplacesBackslashesWithForwardSlashesInEntryNames()
    {
        const string folderPath = @"C:\MyFolder";
        const string outputPath = @"C:\MyFolder.zip";
        const string subDir = @"C:\MyFolder\Sub\Dir";
        const string file = @"C:\MyFolder\Sub\Dir\file.txt";
        var archive = A.Fake<IZipArchive>();

        A.CallTo(() => fileSystem.DirectoryExists(folderPath)).Returns(true);
        A.CallTo(() => fileSystem.GetFolderName(folderPath)).Returns("MyFolder");
        A.CallTo(() => fileSystem.GetParentDirectory(folderPath)).Returns(@"C:\");
        A.CallTo(() => fileSystem.CombinePaths(@"C:\", "MyFolder.zip")).Returns(outputPath);
        A.CallTo(() => fileOperations.FileExists(outputPath)).Returns(false);
        A.CallTo(() => zipArchiveFactory.Open(outputPath, ZipArchiveMode.Create)).Returns(archive);

        // Root - entryPrefix starts as empty
        A.CallTo(() => fileSystem.GetFiles(folderPath, "*.*", SearchOption.TopDirectoryOnly)).Returns([]);
        A.CallTo(() => fileSystem.GetDirectories(folderPath)).Returns([@"C:\MyFolder\Sub"]);
        A.CallTo(() => fileSystem.GetFolderName(@"C:\MyFolder\Sub")).Returns("Sub");
        // newEntryPrefix becomes "Sub"

        // Sub - entryPrefix is "Sub"
        A.CallTo(() => fileSystem.GetFiles(@"C:\MyFolder\Sub", "*.*", SearchOption.TopDirectoryOnly)).Returns([]);
        A.CallTo(() => fileSystem.GetDirectories(@"C:\MyFolder\Sub")).Returns([subDir]);
        A.CallTo(() => fileSystem.GetFolderName(subDir)).Returns("Dir");
        A.CallTo(() => fileSystem.CombinePaths("Sub", "Dir")).Returns(@"Sub\Dir");
        // newEntryPrefix becomes "Sub\Dir".Replace('\\', '/') = "Sub/Dir"

        // Dir - entryPrefix is "Sub/Dir"
        A.CallTo(() => fileSystem.GetFiles(subDir, "*.*", SearchOption.TopDirectoryOnly)).Returns([file]);
        A.CallTo(() => fileOperations.GetFileName(file)).Returns("file.txt");
        A.CallTo(() => fileSystem.CombinePaths("Sub/Dir", "file.txt")).Returns(@"Sub/Dir\file.txt");
        // entryName becomes "Sub/Dir\file.txt".Replace('\\', '/') = "Sub/Dir/file.txt"
        A.CallTo(() => fileSystem.GetDirectories(subDir)).Returns([]);

        compressor.CompressFolder(folderPath);

        A.CallTo(() => archive.CreateEntryFromFile(file, "Sub/Dir/file.txt", CompressionLevel.Optimal))
            .MustHaveHappenedOnceExactly();
    }
}
