using FlowGit.API.DTOs;
using FlowGit.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace FlowGit.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController(IProductService products) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<ProductResponseDto>>> GetAll(
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        return Ok(await products.SearchAsync(search, page, pageSize, cancellationToken));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProductResponseDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var product = await products.GetByIdAsync(id, cancellationToken);
        if (product is null)
            return NotFound();

        return Ok(product);
    }

    [HttpPost]
    public async Task<ActionResult<ProductResponseDto>> Create(CreateProductDto input, CancellationToken cancellationToken)
    {
        var result = await products.CreateAsync(input, cancellationToken);
        if (result.Error is not null)
            return BadRequest(result.Error);

        return CreatedAtAction(nameof(GetById), new { id = result.Product!.Id }, result.Product);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UpdateProductDto input, CancellationToken cancellationToken)
    {
        var result = await products.UpdateAsync(id, input, cancellationToken);
        if (result.NotFound)
            return NotFound();
        if (result.Error is not null)
            return BadRequest(result.Error);

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        if (!await products.DeleteAsync(id, cancellationToken))
            return NotFound();

        return NoContent();
    }
}
