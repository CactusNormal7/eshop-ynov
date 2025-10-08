using FluentValidation;

namespace Catalog.API.Features.Products.Commands.BulkImportProducts;

public class BulkImportProductsCommandValidator : AbstractValidator<BulkImportProductsCommand>
{
    public BulkImportProductsCommandValidator()
    {
        RuleFor(command => command.ExcelFile)
            .NotNull()
            .WithMessage("Excel file is required");

        RuleFor(command => command.ExcelFile.Length)
            .GreaterThan(0)
            .When(command => command.ExcelFile != null)
            .WithMessage("Excel file cannot be empty");

        RuleFor(command => command.ExcelFile.FileName)
            .Must(fileName => fileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase) || 
                              fileName.EndsWith(".xls", StringComparison.OrdinalIgnoreCase))
            .When(command => command.ExcelFile != null)
            .WithMessage("File must be an Excel file (.xlsx or .xls)");

        // size limit
        RuleFor(command => command.ExcelFile.Length)
            .LessThanOrEqualTo(10 * 1024 * 1024) // 10MB limit
            .When(command => command.ExcelFile != null)
            .WithMessage("Excel file size cannot exceed 10MB");
    }
}
