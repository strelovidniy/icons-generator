using System.Text.Json;
using IconsGenerator.Server.Models;
using IconsGenerator.Server.Services.Abstraction;
using Microsoft.AspNetCore.Mvc;

namespace IconsGenerator.Server.Controllers;

[Route("api/images")]
[ApiController]
public class ImageController(
    IImageService imageService
) : ControllerBase
{
    [HttpPost("create-icons")]
    public async Task<IActionResult> CreateIconsAsync(
        CancellationToken cancellationToken = default
    )
    {
        var form = HttpContext.Request.Form;

        if (!form.TryGetValue("sizes", out var stringSizes))
        {
            return BadRequest();
        }

        var sizes = JsonSerializer.Deserialize<List<int>>(stringSizes.ToString()) ?? [];

        var image = form.Files.GetFile("image");

        if (image is null)
        {
            return BadRequest();
        }

        var model = new CreateIconsModel(sizes, image);

        if (model.Image.ContentType
            is not "image/jpeg"
            and not "image/jpg"
            and not "image/png"
            and not "image/webp"
            and not "image/x-MS-bmp"
            and not "image/tiff"
            and not "image/tif"
            and not "image/gif"
            and not "image/x-portable-bitmap")
        {
            return BadRequest();
        }

        if (!model.Sizes.Any())
        {
            return BadRequest();
        }

        var result = await imageService.GenerateIconsAsync(model, cancellationToken);

        return File(result, "application/octet-stream");
    }
}