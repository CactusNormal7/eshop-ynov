using BuildingBlocks.CQRS;

namespace Catalog.API.Features.Products.Queries.GetProductsByCategory;

public record GetProductsByCategoryQuery(string Category) : IQuery<GetProductsByCategoryResult>;