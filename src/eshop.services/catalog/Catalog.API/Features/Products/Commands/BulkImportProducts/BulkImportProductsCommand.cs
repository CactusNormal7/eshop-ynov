using BuildingBlocks.CQRS;
using Microsoft.AspNetCore.Http;

namespace Catalog.API.Features.Products.Commands.BulkImportProducts;

public class BulkImportProductsCommand : ICommand<BulkImportProductsCommandResult>
{
    public required IFormFile ExcelFile { get; set; }
}
