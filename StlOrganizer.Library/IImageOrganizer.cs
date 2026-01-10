namespace StlOrganizer.Library;

public interface IImageOrganizer
{
    Task<int> OrganizeImagesAsync(string rootPath);
}
