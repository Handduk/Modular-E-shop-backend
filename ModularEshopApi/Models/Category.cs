using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace ModularEshopApi.Models
{
    public class Category
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; }
        public string? Image { get; set; }
        public List<Product>? Products { get; set; } = new List<Product>();
    }
}
