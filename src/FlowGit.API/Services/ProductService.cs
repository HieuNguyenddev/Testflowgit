using FlowGit.API.DTOs;
using FlowGit.API.Entities;
using FlowGit.API.Repositories;

namespace FlowGit.API.Services;

public class ProductService(IProductRepository products) : IProductService
{
    public async Task<PagedResult<ProductResponseDto>> SearchAsync(
        string? search,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize is < 1 or > 50 ? 10 : pageSize;

        var (items, totalCount) = await products.SearchAsync(search, page, pageSize, cancellationToken);
        return new PagedResult<ProductResponseDto>(items.Select(ToResponse).ToList(), page, pageSize, totalCount);
    }

    public async Task<ProductResponseDto?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var product = await products.GetByIdAsync(id, tracking: false, cancellationToken);
        return product is null ? null : ToResponse(product);
    }

    public async Task<ProductWriteResult> CreateAsync(CreateProductDto input, CancellationToken cancellationToken)
    {
        var error = Validate(input.Name, input.Price, input.Quantity);
        if (error is not null)
            return ProductWriteResult.Invalid(error);

        var product = new Product
        {
            Name = input.Name.Trim(),
            Price = input.Price,
            Quantity = input.Quantity
        };

        products.Add(product);
        await products.SaveChangesAsync(cancellationToken);
        return ProductWriteResult.Success(ToResponse(product));
    }

    public async Task<ProductWriteResult> UpdateAsync(int id, UpdateProductDto input, CancellationToken cancellationToken)
    {
        var product = await products.GetByIdAsync(id, tracking: true, cancellationToken);
        if (product is null)
            return ProductWriteResult.Missing();

        var error = Validate(input.Name, input.Price, input.Quantity);
        if (error is not null)
            return ProductWriteResult.Invalid(error);

        products.Update(product, input.Name.Trim(), input.Price, input.Quantity);
        await products.SaveChangesAsync(cancellationToken);
        return ProductWriteResult.Success();
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken)
    {
        var product = await products.GetByIdAsync(id, tracking: true, cancellationToken);
        if (product is null)
            return false;

        products.Remove(product);
        await products.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static string? Validate(string name, decimal price, int quantity) =>
        string.IsNullOrWhiteSpace(name) || price < 0 || quantity < 0
            ? "Invalid product information."
            : null;

    private static ProductResponseDto ToResponse(Product product) => new()
    {
        Id = product.Id,
        Name = product.Name,
        Price = product.Price,
        Quantity = product.Quantity
    };
}
