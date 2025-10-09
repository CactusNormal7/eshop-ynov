using BuildingBlocks.CQRS;
using Catalog.API.Exceptions;
using Catalog.API.Models;
using Marten;
using OfficeOpenXml;

namespace Catalog.API.Features.Products.Commands.BulkImportProducts;

public class BulkImportProductsCommandHandler(IDocumentSession documentSession)
    : ICommandHandler<BulkImportProductsCommand, BulkImportProductsCommandResult>
{
    public async Task<BulkImportProductsCommandResult> Handle(BulkImportProductsCommand request,
        CancellationToken cancellationToken)
    {
        var result = new BulkImportProductsCommandResult();
        var importedProductIds = new List<Guid>();
        var errors = new List<string>();

        try
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using var stream = request.ExcelFile.OpenReadStream();
            using var package = new ExcelPackage(stream);

            var worksheet = package.Workbook.Worksheets.FirstOrDefault();
            if (worksheet == null)
            {
                errors.Add("No worksheet found in the Excel file");
                result.Errors = errors;
                return result;
            }

            var headerRow = 1;
            var expectedHeaders = new[] { "Name", "Description", "Price", "ImageFile", "Categories" };
            var actualHeaders = new List<string>();

            for (int col = 1; col <= expectedHeaders.Length; col++)
            {
                actualHeaders.Add(worksheet.Cells[headerRow, col].Text?.Trim() ?? "");
            }

            if (!expectedHeaders.SequenceEqual(actualHeaders, StringComparer.OrdinalIgnoreCase))
            {
                errors.Add(
                    $"Invalid Excel format. Expected headers: {string.Join(", ", expectedHeaders)}. Found: {string.Join(", ", actualHeaders)}");
                result.Errors = errors;
                return result;
            }

            var rowCount = worksheet.Dimension?.Rows ?? 0;
            result.TotalProcessed = rowCount - 1; // on exclude la header row

            for (int row = 2; row <= rowCount; row++)
            {
                try
                {
                    var product = ExtractProductFromRow(worksheet, row);
                    if (product == null)
                    {
                        errors.Add($"Row {row}: Invalid product data");
                        result.FailedImports++;
                        continue;
                    }

                    var existingProduct = await documentSession.Query<Product>()
                        .FirstOrDefaultAsync(
                            x => x.Name.Equals(product.Name, StringComparison.CurrentCultureIgnoreCase),
                            cancellationToken);

                    if (existingProduct != null)
                    {
                        errors.Add($"Row {row}: Product '{product.Name}' already exists");
                        result.FailedImports++;
                        continue;
                    }

                    documentSession.Store(product);
                    importedProductIds.Add(product.Id);
                    result.SuccessfullyImported++;
                }
                catch (Exception ex)
                {
                    errors.Add($"Row {row}: {ex.Message}");
                    result.FailedImports++;
                }
            }

            if (result.SuccessfullyImported > 0)
            {
                await documentSession.SaveChangesAsync(cancellationToken);
            }
        }
        catch (Exception ex)
        {
            errors.Add($"Error processing Excel file: {ex.Message}");
        }

        result.Errors = errors;
        result.ImportedProductIds = importedProductIds;
        return result;
    }

    private static Product? ExtractProductFromRow(ExcelWorksheet worksheet, int row)
    {
        try
        {
            var name = worksheet.Cells[row, 1].Value?.ToString()?.Trim();
            var description = worksheet.Cells[row, 2].Value?.ToString()?.Trim();
            var priceValue = worksheet.Cells[row, 3].Value;
            var imageFile = worksheet.Cells[row, 4].Value?.ToString()?.Trim();
            var categoriesText = worksheet.Cells[row, 5].Value?.ToString()?.Trim();

            Console.WriteLine(
                $"Row {row}: Name='{name}', Description='{description}', Price='{priceValue}', Image='{imageFile}', Categories='{categoriesText}'");

            if (string.IsNullOrWhiteSpace(name) ||
                string.IsNullOrWhiteSpace(description) ||
                priceValue == null ||
                string.IsNullOrWhiteSpace(imageFile) ||
                string.IsNullOrWhiteSpace(categoriesText))
            {
                Console.WriteLine($"Row {row}: Validation failed - missing required fields");
                return null;
            }

            decimal price;
            if (priceValue is decimal decimalValue)
            {
                price = decimalValue;
            }
            else if (priceValue is double doubleValue)
            {
                price = (decimal)doubleValue;
            }
            else if (priceValue is int intValue)
            {
                price = intValue;
            }
            else if (decimal.TryParse(priceValue.ToString(), out price))
            {
            }
            else
            {
                Console.WriteLine($"Row {row}: Invalid price format: {priceValue}");
                return null;
            }

            if (price <= 0)
            {
                Console.WriteLine($"Row {row}: Price must be greater than 0: {price}");
                return null;
            }

            var categories = categoriesText.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(c => c.Trim())
                .Where(c => !string.IsNullOrWhiteSpace(c))
                .ToList();

            if (!categories.Any())
            {
                Console.WriteLine($"Row {row}: No valid categories found");
                return null;
            }

            var product = new Product
            {
                Id = Guid.NewGuid(),
                Name = name,
                Description = description,
                Price = price,
                ImageFile = imageFile,
                Categories = categories
            };

            Console.WriteLine($"Row {row}: Successfully created product: {product.Name}");
            return product;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Row {row}: Exception occurred: {ex.Message}");
            return null;
        }
    }
}