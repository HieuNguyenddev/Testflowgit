using FlowGit.API.Data;
using FlowGit.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace FlowGit.API.Repositories;

public class ProductRepository(AppDbContext db) : IProductRepository
{
    public async Task<(IReadOnlyList<Product> Items, int TotalCount)> SearchAsync(
        string? search,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var query = db.Products.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            foreach (var term in search.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                var pattern = $"%{EscapeLike(TextSearch.Normalize(term))}%";
                query = query.Where(p => EF.Functions.Like(EF.Property<string>(p, "NameSearch"), pattern));
            }
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(p => p.Name)
            .ThenBy(p => p.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public Task<Product?> GetByIdAsync(int id, bool tracking, CancellationToken cancellationToken)
    {
        var query = tracking ? db.Products : db.Products.AsNoTracking();
        return query.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public void Add(Product product)
    {
        db.Products.Add(product);
        db.Entry(product).Property("NameSearch").CurrentValue = TextSearch.Normalize(product.Name);
    }

    public void Update(Product product, string name, decimal price, int quantity)
    {
        product.Name = name;
        product.Price = price;
        product.Quantity = quantity;
        db.Entry(product).Property("NameSearch").CurrentValue = TextSearch.Normalize(name);
    }

    public void Remove(Product product) => db.Products.Remove(product);

    public Task SaveChangesAsync(CancellationToken cancellationToken) =>
        db.SaveChangesAsync(cancellationToken);

    private static string EscapeLike(string value) =>
        value.Replace("[", "[[]", StringComparison.Ordinal)
            .Replace("%", "[%]", StringComparison.Ordinal)
            .Replace("_", "[_]", StringComparison.Ordinal);
}
