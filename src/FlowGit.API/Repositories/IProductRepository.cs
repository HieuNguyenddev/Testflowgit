using FlowGit.API.Entities;

namespace FlowGit.API.Repositories;

public interface IProductRepository
{
    Task<(IReadOnlyList<Product> Items, int TotalCount)> SearchAsync(
        string? search,
        int page,
        int pageSize,
        CancellationToken cancellationToken);

    Task<Product?> GetByIdAsync(int id, bool tracking, CancellationToken cancellationToken);

    void Add(Product product);

    void Update(Product product, string name, decimal price, int quantity);

    void Remove(Product product);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
