using BuildingBlocks.CQRS;

namespace Catalog.API.Features.Products.Commands.ExportProduct;

public record ExportProductsCommand(string[] Categories, int MaxPrice, int MinPrice) : ICommand<ExportProductsCommandResult>;
