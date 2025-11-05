using Catalog.API.Models;

namespace Catalog.API.Features.Products.Queries.GetProductsByCategory;

public record GetProductsByCategoryResult(IEnumerable<Product> Products);