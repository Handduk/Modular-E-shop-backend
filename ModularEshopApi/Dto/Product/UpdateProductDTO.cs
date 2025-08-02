namespace ModularEshopApi.Dto.Product
{
    public class UpdateProductDTO
    {
        public string? Brand { get; set; }
        public required string Name { get; set; }
        public required string Description { get; set; }
        public List<string>? Options { get; set; }
        public decimal Price { get; set; }
        public List<Variant>? Variants { get; set; }
        public decimal? Discount { get; set; }
        // Uploaded files from the frontend
        public List<IFormFile> NewImages { get; set; } = new();
        // Remove image from the product
        public List<string> KeptImages { get; set; } = new();
    }
}
