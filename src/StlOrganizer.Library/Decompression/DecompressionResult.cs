namespace StlOrganizer.Library.Decompression;

public record DecompressionResult(
    IEnumerable<string> ExtractedFiles,
    IEnumerable<string> CompressedFiles);
