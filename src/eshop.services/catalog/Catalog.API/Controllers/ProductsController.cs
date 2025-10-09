using Catalog.API.Features.Products.Commands.BulkImportProducts;
using Catalog.API.Features.Products.Commands.CreateProduct;
using Catalog.API.Features.Products.Commands.DeleteProduct;
using Catalog.API.Features.Products.Commands.ExportProduct;
using Catalog.API.Features.Products.Commands.UpdateProduct;
using Catalog.API.Features.Products.Queries.GetProductById;
using Catalog.API.Features.Products.Queries.GetProductsByPagination;
using Catalog.API.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Catalog.API.Controllers;

/// <summary>
/// Manages operations related to products within the catalog, including retrieving product data
/// and creating new products.
/// </summary>
[ApiController]
[Route("[controller]")]
[Produces("application/json")]
public class ProductsController(ISender sender) : ControllerBase
{
    /// <summary>
    /// Retrieves a product by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the product to retrieve.</param>
    /// <returns>The product matching the specified identifier, if found; otherwise, a not found response.</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(Product), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(NotFoundObjectResult), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Product>> GetProductById(Guid id)
    {
        var result = await sender.Send(new GetProductByIdQuery(id));
        return Ok(result.Product);

    }

    /// <summary>
    /// Retrieves a collection of products within a specified category.
    /// </summary>
    /// <param name="category">The category by which to filter the products.</param>
    /// <returns>A collection of products belonging to the specified category, if found; otherwise, a bad request response.</returns>
    [HttpGet("category/{category}")]
    [ProducesResponseType(typeof(IEnumerable<Product>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BadRequestObjectResult), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Product>> GetProductsByCategory(string category)
    {
        // TODO
        if (string.IsNullOrWhiteSpace(category))
            return BadRequest("Category is required");

        var result = await sender.Send(new());
        return Ok();
    }

    /// <summary>
    /// Retrieves a paginated collection of products from the catalog.
    /// </summary>
    /// <param name="pageNumber">The 1-based page number to retrieve. Defaults to 1.</param>
    /// <param name="pageSize">The number of items per page. Defaults to 10.</param>
    /// <remarks>
    /// Usage:
    /// - GET /products?pageNumber=1&amp;pageSize=10
    /// - If omitted, defaults are applied: pageNumber=1, pageSize=10.
    ///
    /// Example curl:
    /// curl -X GET "https://localhost:5001/products?pageNumber=2&amp;pageSize=20" -H  "accept: application/json"
    ///
    /// The response body contains pagination metadata (PageIndex, PageSize, TotalCount) and the Data array.
    /// </remarks>
    /// <returns>A paginated result containing products and pagination metadata.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(BuildingBlocks.Pagination.PaginatedResult<Product>), StatusCodes.Status200OK)]
    public async Task<ActionResult<BuildingBlocks.Pagination.PaginatedResult<Product>>> GetProducts(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] int maxPrice = 0,
        [FromQuery] int minPrice = 0,
        [FromQuery] string[] categories = null)
    {
        var result =
            await sender.Send(new GetProductsByPaginationQuery(pageNumber, pageSize, categories, maxPrice, minPrice));
        return Ok(result.Products);
    }

    /// <summary>
    /// Handles the creation of a new product.
    /// </summary>
    /// <param name="request">The command containing the details of the product to be created.</param>
    /// <returns>A result object containing the ID of the newly created product.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(CreateProductCommandResult), StatusCodes.Status201Created)]
    public async Task<ActionResult<CreateProductCommandResult>> CreateProduct(CreateProductCommand request)
    {
        var result = await sender.Send(request);
        return CreatedAtAction(nameof(GetProductById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Updates a product with the specified ID using the provided update details.
    /// </summary>
    /// <param name="id">The unique identifier of the product to update.</param>
    /// <param name="request">The details to update the specified product.</param>
    /// <returns>A boolean indicating whether the update was successful or an appropriate error response if the product was not found.</returns>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(NotFoundObjectResult), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<bool>> UpdateProduct(Guid id, [FromBody] UpdateProductCommand request)
    {
        if (id != request.Id)
        {
            return BadRequest("ID in URL does not match ID in body");
        }

        var result = await sender.Send(request);
        return Ok(result.IsSuccessful);
    }

    /// <summary>
    /// Deletes a product by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the product to delete.</param>
    /// <returns>True if the product was successfully deleted; otherwise, a not found response.</returns>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(NotFoundObjectResult), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Product>> DeleteProduct(Guid id)
    {
        var result = await sender.Send(new DeleteProductCommand(id));
        return Ok(result.IsSuccessful);
    }

    /// <summary>
    /// Imports multiple products from an Excel file.
    /// </summary>
    /// <param name="excelFile">The Excel file containing product data.</param>
    /// <returns>A result object containing the import statistics and any errors encountered.</returns>
    [HttpPost("bulk-import")]
    [ProducesResponseType(typeof(BulkImportProductsCommandResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BadRequestObjectResult), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<BulkImportProductsCommandResult>> BulkImportProducts(IFormFile excelFile)
    {
        if (excelFile == null || excelFile.Length == 0)
        {
            return BadRequest("Excel file is required");
        }

        var command = new BulkImportProductsCommand { ExcelFile = excelFile };
        var result = await sender.Send(command);

        return Ok(result);
    }

    /// <summary>
    /// Exports all products to an Excel file.
    /// </summary>
    /// <returns>An Excel file containing all products from the database.</returns>
    [HttpGet("export-all")]
    [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BadRequestObjectResult), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ExportAllProducts()
    {
        var command = new ExportAllProductsCommand();
        var result = await sender.Send(command);
        
        return File(
            result.ExcelFileBytes, 
            result.ContentType, 
            result.FileName);
    }
}