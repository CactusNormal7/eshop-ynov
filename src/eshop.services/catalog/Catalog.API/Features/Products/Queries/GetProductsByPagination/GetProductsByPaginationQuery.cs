using BuildingBlocks.CQRS;

namespace Catalog.API.Features.Products.Queries.GetProductsByPagination;

public record GetProductsByPaginationQuery(int PageIndex, int PageSize) : IQuery<GetProductByPaginationQueryResult>;