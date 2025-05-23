using ModularEshopApi.Dto.Product;

namespace ModularEshopApi.Dto.Category
{
    public class GetCategoryDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public List<GetProductsDTO>? Products { get; set; }
    }
}
