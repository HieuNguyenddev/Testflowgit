using FlowGit.API.DTOs;

namespace FlowGit.API.Services;

public interface IProductService
{
    Task<PagedResult<ProductResponseDto>> SearchAsync(
        string? search,
        int page,
        int pageSize,
        CancellationToken cancellationToken);

    Task<ProductResponseDto?> GetByIdAsync(int id, CancellationToken cancellationToken);

    Task<ProductWriteResult> CreateAsync(CreateProductDto input, CancellationToken cancellationToken);

    Task<ProductWriteResult> UpdateAsync(int id, UpdateProductDto input, CancellationToken cancellationToken);

    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken);
}

public sealed record ProductWriteResult(ProductResponseDto? Product, string? Error = null, bool NotFound = false)
{
    public static ProductWriteResult Invalid(string error) => new(null, error);

    public static ProductWriteResult Missing() => new(null, NotFound: true);

    public static ProductWriteResult Success(ProductResponseDto? product = null) => new(product);
}
