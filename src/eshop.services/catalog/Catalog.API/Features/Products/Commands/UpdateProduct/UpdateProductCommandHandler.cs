using BuildingBlocks.CQRS;
using Catalog.API.Exceptions;
using Catalog.API.Models;
using Marten;

namespace Catalog.API.Features.Products.Commands.UpdateProduct;

public class UpdateProductCommandHandler(IDocumentSession documentSession) : ICommandHandler<UpdateProductCommand, UpdateProductCommandResult>
{
    public async Task<UpdateProductCommandResult> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await documentSession.LoadAsync<Product>(request.Id, cancellationToken);

        if (product is null)
            throw new ProductNotFoundException(request.Id);

        var nameConflict = await documentSession.Query<Product>()
            .AnyAsync(x => x.Id != request.Id && x.Name.Equals(request.Name, StringComparison.CurrentCultureIgnoreCase), cancellationToken);

        if (nameConflict)
            throw new ProductAlreadyExistsException(request.Name);

        product.Name = request.Name;
        product.Description = request.Description;
        product.Price = request.Price;
        product.ImageFile = request.ImageFile;
        product.Categories = request.Categories;

        documentSession.Store(product);
        await documentSession.SaveChangesAsync(cancellationToken);

        return new UpdateProductCommandResult(true);
    }
}

