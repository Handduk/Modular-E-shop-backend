using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace ModularEshopApi.Models
{
    public class Product
    {
        public int Id { get; set; }
        public int CategoryId { get; set; }
        public string? Brand { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public List<string>? Options { get; set; }
        public decimal Price { get; set; }
        public List<string>? Variants { get; set; }
        public decimal? Discount { get; set; }
        public List<string>? Images { get; set; }
    }
}
