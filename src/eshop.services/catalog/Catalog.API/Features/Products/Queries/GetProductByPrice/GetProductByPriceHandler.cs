using Marten;
using Catalog.API.Models;
using BuildingBlocks.CQRS;
using Catalog.API.Exceptions;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Catalog.API.Features.Products.Queries.GetProductByPrice;

public class GetProductByPriceHandler : IQueryHandler<GetProductByPriceQuery, GetProductByPriceResult>
{
    private readonly IDocumentSession _documentSession;

    public GetProductByPriceHandler(IDocumentSession documentSession)
    {
        _documentSession = documentSession;
    }

    public async Task<GetProductByPriceResult> Handle(GetProductByPriceQuery request, CancellationToken cancellationToken)
    {
        var products = await _documentSession.Query<Product>()
            .Where(p => p.Price == request.price)
            .ToListAsync(cancellationToken);

        if (products == null || !products.Any())
        {
            throw new ProductNotFoundException(request.price);
        }

        return new GetProductByPriceResult(products);
    }
}