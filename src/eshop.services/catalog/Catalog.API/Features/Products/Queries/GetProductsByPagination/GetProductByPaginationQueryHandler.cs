using Catalog.API.Extensions;

namespace Catalog.API.Features.Products.Queries.GetProductsByPagination;

using BuildingBlocks.CQRS;
using BuildingBlocks.Pagination;
using Catalog.API.Models;
using Marten;
using Marten.Pagination;

public class GetProductByPaginationQueryHandler(IDocumentSession documentSession)
    : IQueryHandler<GetProductsByPaginationQuery, GetProductByPaginationQueryResult>
{
    public Task<GetProductByPaginationQueryResult> Handle(GetProductsByPaginationQuery request,
        CancellationToken cancellationToken)
    {
        var filteredDb = documentSession.Query<Product>();
        filteredDb = filteredDb.ApplyFilterOnProduct(request.Categories, request.MaxPrice, request.MinPrice);        

        var pagedList = filteredDb
            .ToPagedList(request.PageIndex, request.PageSize);

        var result = new PaginatedResult<Product>(
            (int)pagedList.PageNumber,
            (int)pagedList.PageSize,
            (long)pagedList.TotalItemCount,
            pagedList.ToArray());

        return Task.FromResult(new GetProductByPaginationQueryResult(result));
    }
}