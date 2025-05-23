namespace ModularEshopApi.Dto.Product
{
    public class GetProductsDTO
    {
        public int Id { get; set; }
        public int CategoryId { get; set; }
        public string? Brand { get; set; }
        public required string Name { get; set; }
        public required string Description { get; set; }
        public List<string>? Options { get; set; }
        public decimal Price { get; set; }
        public List<string>? Variants { get; set; }
        public decimal? Discount { get; set; }

        public List<string>? Images { get; set; }
    }
}
