using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace PassKee.Business.Helpers;

public static class ImageHelper
{
    public static async Task<Image> ResizeImageFromStreamAsync(string path, int width, int height)
    {
        using var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read);
        return await ResizeImageFromStreamAsync(fileStream, width, height);
    }
    
    public static async Task<Image> ResizeImageFromStreamAsync(Stream imageStream, int width, int height)
    {
        var image = await Image.LoadAsync(imageStream);
        image.Mutate(
            x => x
                .AutoOrient()
                .Resize(new ResizeOptions
                {
                    Mode = ResizeMode.Max,
                    Position = AnchorPositionMode.Center,
                    Size = new Size(width, height)
                })
                .Grayscale()
        );
        return image;
    }
    
    public static async Task<bool> IsImage(byte[] imageBytes)
    {
        try
        {
            using var stream = new MemoryStream(imageBytes);
            await Image.LoadAsync(stream);
        }
        catch (Exception)
        {
            return false;
        }
        return true;
    }
}
