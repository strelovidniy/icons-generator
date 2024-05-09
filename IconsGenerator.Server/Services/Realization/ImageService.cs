using System.IO.Compression;
using IconsGenerator.Server.Models;
using IconsGenerator.Server.Services.Abstraction;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace IconsGenerator.Server.Services.Realization;

public class ImageService : IImageService
{
    public async Task<byte[]> GenerateIconsAsync(
        CreateIconsModel model,
        CancellationToken cancellationToken = default
    )
    {
        await using var stream = new MemoryStream();

        var files = new Dictionary<string, byte[]>();

        foreach (var size in model.Sizes)
        {
            await stream.FlushAsync(cancellationToken);

            stream.Position = 0;

            await model.Image.CopyToAsync(stream, cancellationToken);

            stream.Position = 0;

            using var image = await Image.LoadAsync(stream, cancellationToken);

            image.Mutate(imageProcessingContext => imageProcessingContext.Resize(
                new ResizeOptions
                {
                    Size = new Size(size, size),
                    Compand = true,
                    Mode = ResizeMode.Stretch,
                    Position = AnchorPositionMode.Center,
                    PadColor = Color.Black,
                    Sampler = KnownResamplers.Bicubic,
                    PremultiplyAlpha = false
                }
            ));

            await stream.FlushAsync(cancellationToken);

            stream.Position = 0;

            await image.SaveAsPngAsync(stream, cancellationToken);

            files.Add($"icon-{size}x{size}.png", stream.ToArray());
        }

        await stream.FlushAsync(cancellationToken);
        await stream.DisposeAsync();

        await using var archiveStream = new MemoryStream();

        await archiveStream.FlushAsync(cancellationToken);
        archiveStream.Position = 0;

        using var archive = new ZipArchive(archiveStream, ZipArchiveMode.Create, true);

        foreach (var file in files)
        {
            var zipArchiveEntry = archive.CreateEntry(file.Key, CompressionLevel.NoCompression);
            await using var zipStream = zipArchiveEntry.Open();

            await zipStream.WriteAsync(file.Value, cancellationToken);
        }

        archive.Dispose();

        archiveStream.Position = 0;

        var result = archiveStream.ToArray();

        await archiveStream.FlushAsync(cancellationToken);

        await archiveStream.DisposeAsync();

        GC.Collect();

        return result;
    }
}