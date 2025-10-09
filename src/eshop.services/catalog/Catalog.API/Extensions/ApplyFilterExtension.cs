using Catalog.API.Models;
using JasperFx.Core.Reflection;
using Marten.Linq;

namespace Catalog.API.Extensions;

public static class ApplyFilterExtension
{
    public static IMartenQueryable<Product> ApplyFilterOnProduct(
        this IMartenQueryable<Product> query,
        string[]? categories = null,
        int? maxPrice = null,
        int? minPrice = null)
    {
        if (categories is { Length: > 0 })
        {
            // query = query.Where(x => x.Categories.Any(c => categories.Contains(c))).As<IMartenQueryable<Product>>();
        }

        if (minPrice.HasValue)
        {
            query = query.Where(x => x.Price >= minPrice.Value).As<IMartenQueryable<Product>>();
        }

        if (maxPrice.HasValue)
        {
            query = query.Where(x => x.Price <= maxPrice.Value).As<IMartenQueryable<Product>>();
        }

        return query;
    }
}