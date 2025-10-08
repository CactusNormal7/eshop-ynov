namespace Catalog.API.Features.Products.Commands.BulkImportProducts;

public class BulkImportProductsCommandResult
{
    public int TotalProcessed { get; set; }

    public int SuccessfullyImported { get; set; }

    public int FailedImports { get; set; }

    public List<string> Errors { get; set; } = [];

    public List<Guid> ImportedProductIds { get; set; } = [];

    public BulkImportProductsCommandResult()
    {
    }

    public BulkImportProductsCommandResult(int totalProcessed, int successfullyImported, int failedImports)
    {
        TotalProcessed = totalProcessed;
        SuccessfullyImported = successfullyImported;
        FailedImports = failedImports;
    }
}
