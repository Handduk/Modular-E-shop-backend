using ModularEshopApi.Models;

namespace ModularEshopApi.Dto.Product
{
    public class CreateProductDTO
    {
        public int CategoryId { get; set; }
        public string? Brand { get; set; }
        public required string Name { get; set; }
        public required string Description { get; set; }
        public List<string>? Options { get; set; }
        public decimal Price { get; set; }
        public decimal? Discount { get; set; }

        public List<IFormFile>? Images { get; set; }

        public string? VariantJson { get; set; }
        public List<IFormFile> VariantImages { get; set; } = new();
    }
}
