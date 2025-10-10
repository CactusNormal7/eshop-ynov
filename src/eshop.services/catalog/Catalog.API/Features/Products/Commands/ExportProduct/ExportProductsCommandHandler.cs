using Catalog.API.Extensions;

using BuildingBlocks.CQRS;
using Catalog.API.Models;
using Marten;
using OfficeOpenXml;

namespace Catalog.API.Features.Products.Commands.ExportProduct;

public class ExportProductsCommandHandler(IDocumentSession documentSession)
    : ICommandHandler<ExportProductsCommand, ExportProductsCommandResult>
{
    public async Task<ExportProductsCommandResult> Handle(ExportProductsCommand request,
        CancellationToken cancellationToken)
    {
        var filteredDb = documentSession.Query<Product>();
        filteredDb = filteredDb.ApplyFilterOnProduct(request.Categories, request.MaxPrice, request.MinPrice);

        var products = await filteredDb.ToListAsync(cancellationToken);

        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

        try
        {
            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Products");

            worksheet.Cells[1, 1].Value = "Name";
            worksheet.Cells[1, 2].Value = "Description";
            worksheet.Cells[1, 3].Value = "Price";
            worksheet.Cells[1, 4].Value = "Stock";
            worksheet.Cells[1, 5].Value = "ImageFile";
            worksheet.Cells[1, 6].Value = "Categories";

            using (var range = worksheet.Cells[1, 1, 1, 6])
            {
                range.Style.Font.Bold = true;
                range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);
            }

            for (int i = 0; i < products.Count; i++)
            {
                var product = products[i];
                var row = i + 2;

                worksheet.Cells[row, 1].Value = product.Name ?? string.Empty;
                worksheet.Cells[row, 2].Value = product.Description ?? string.Empty;
                worksheet.Cells[row, 3].Value = product.Price;
                worksheet.Cells[row, 4].Value = product.Stock;
                worksheet.Cells[row, 5].Value = product.ImageFile ?? string.Empty;
                worksheet.Cells[row, 6].Value = string.Join(", ",
                    (IEnumerable<string>?)product.Categories ?? System.Array.Empty<string>());
            }

            worksheet.Cells.AutoFitColumns();

            var fileBytes = package.GetAsByteArray();

            return new ExportProductsCommandResult
            {
                ExcelFileBytes = fileBytes,
                FileName = $"export_produits_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx",
                ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                TotalProductsExported = products.Count
            };
        }
        catch (OperationCanceledException)
        {
            return new ExportProductsCommandResult
            {
                ExcelFileBytes = System.Array.Empty<byte>(),
                FileName = $"export_produits_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx",
                ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                TotalProductsExported = 0
            };
        }
        catch (Exception)
        {
            return new ExportProductsCommandResult
            {
                ExcelFileBytes = System.Array.Empty<byte>(),
                FileName = $"export_produits_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx",
                ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                TotalProductsExported = 0
            };
        }
    }
}