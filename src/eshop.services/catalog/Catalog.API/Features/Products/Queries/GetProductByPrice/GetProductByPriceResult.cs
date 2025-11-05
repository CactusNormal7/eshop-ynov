using Catalog.API.Models;

namespace Catalog.API.Features.Products.Queries.GetProductByPrice;

public record GetProductByPriceResult(IEnumerable<Product> Products);