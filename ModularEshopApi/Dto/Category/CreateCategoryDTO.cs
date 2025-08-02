using ModularEshopApi.Models;

namespace ModularEshopApi.Dto.Category
{
    public class CreateCategoryDTO
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; }
        public IFormFile? Image { get; set; }
    }
}
