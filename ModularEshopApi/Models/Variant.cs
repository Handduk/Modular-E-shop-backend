namespace ModularEshopApi.Models;

public class Variant
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string? VariantName { get; set; }
    public decimal VariantPrice { get; set; }

    public string? VariantImg { get; set; }

}