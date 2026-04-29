namespace ProductManagement.Services.DTOs;

public class UpdateProductDTO
{
    public required string Name { get; set; }
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public string? Category { get; set; }
    public int StockQuantity { get; set; }
}