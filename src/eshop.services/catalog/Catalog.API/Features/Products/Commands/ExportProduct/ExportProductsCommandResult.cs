namespace Catalog.API.Features.Products.Commands.ExportProduct;

public class ExportProductsCommandResult
{
    public required byte[] ExcelFileBytes { get; set; }
    public required string FileName { get; set; }
    public required string ContentType { get; set; }
    public int TotalProductsExported { get; set; }
}
