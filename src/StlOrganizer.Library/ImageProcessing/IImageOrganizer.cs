namespace StlOrganizer.Library.ImageProcessing;

public interface IImageOrganizer
{
    Task<int> OrganizeImagesAsync(string rootPath);
}
