namespace IconsGenerator.Server.Models;

public record CreateIconsModel(
    IEnumerable<int> Sizes,
    IFormFile Image
);