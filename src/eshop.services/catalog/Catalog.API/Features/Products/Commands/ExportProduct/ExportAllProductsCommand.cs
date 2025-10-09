using BuildingBlocks.CQRS;

namespace Catalog.API.Features.Products.Commands.ExportProduct;

public record ExportAllProductsCommand : ICommand<ExportAllProductsCommandResult>;
