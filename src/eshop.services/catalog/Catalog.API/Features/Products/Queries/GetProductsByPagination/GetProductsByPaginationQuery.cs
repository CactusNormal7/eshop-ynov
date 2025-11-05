using BuildingBlocks.CQRS;

namespace Catalog.API.Features.Products.Queries.GetProductsByPagination;

public record GetProductsByPaginationQuery(int PageIndex, int PageSize, string[] Categories, int MaxPrice, int MinPrice) : IQuery<GetProductByPaginationQueryResult>;