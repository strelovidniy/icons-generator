using IconsGenerator.Server.Models;

namespace IconsGenerator.Server.Services.Abstraction;

public interface IImageService
{
    public Task<byte[]> GenerateIconsAsync(
        CreateIconsModel model,
        CancellationToken cancellationToken = default
    );
}