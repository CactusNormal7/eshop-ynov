using BuildingBlocks.Pagination;
using Catalog.API.Models;

namespace Catalog.API.Features.Products.Queries.GetProductsByPagination;

public record GetProductByPaginationQueryResult(PaginatedResult<Product> Products);