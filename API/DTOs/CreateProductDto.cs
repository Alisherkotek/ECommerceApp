using System.ComponentModel.DataAnnotations;

namespace API.DTOs;

public class CreateProductDto
{
    [Required] public string Name { get; init; } = string.Empty;
    [Required] public string Description { get; set; } = string.Empty;

    [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than zero.")]
    public decimal Price { get; set; }

    [Required] public string PictureUrl { get; set; } = string.Empty;
    [Required] public string Type { get; set; } = string.Empty;
    [Required] public required string Brand { get; set; }

    [Range(1, double.MaxValue, ErrorMessage = "Quantity must be greater than at least 1.")]
    public int QuantityInStock { get; set; }
}