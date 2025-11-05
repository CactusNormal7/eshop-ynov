using BuildingBlocks.CQRS;

namespace Catalog.API.Features.Products.Queries.GetProductByPrice;

public record GetProductByPriceQuery(decimal price) : IQuery<GetProductByPriceResult>;