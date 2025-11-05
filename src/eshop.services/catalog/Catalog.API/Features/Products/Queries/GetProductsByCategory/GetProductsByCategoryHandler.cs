using Marten;
using Catalog.API.Models;
using BuildingBlocks.CQRS;
using Catalog.API.Exceptions;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Catalog.API.Features.Products.Queries.GetProductsByCategory;

public class GetProductsByCategoryHandler : IQueryHandler<GetProductsByCategoryQuery, GetProductsByCategoryResult>
{
    private readonly IDocumentSession _documentSession;

    public GetProductsByCategoryHandler(IDocumentSession documentSession)
    {
        _documentSession = documentSession;
    }

    public async Task<GetProductsByCategoryResult> Handle(GetProductsByCategoryQuery request, CancellationToken cancellationToken)
    {
        var products = await _documentSession.Query<Product>()
            .Where(p => p.Categories.Any(c => c == request.Category))
            .ToListAsync(cancellationToken);

        if (products == null || !products.Any())
        {
            throw new ProductNotFoundException(request.Category);
        }

        return new GetProductsByCategoryResult(products);
    }
}
