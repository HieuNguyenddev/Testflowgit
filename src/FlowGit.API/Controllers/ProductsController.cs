
using FlowGit.API.Entities;
using Microsoft.AspNetCore.Mvc;

namespace FlowGit.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private static readonly List<Product> Products =
    [
        new() { Id = 1, Name = "Bàn phím cơ", Price = 1_290_000m, Quantity = 15 },
        new() { Id = 2, Name = "Chuột không dây", Price = 450_000m, Quantity = 30 },
        new() { Id = 3, Name = "Màn hình 27 inch", Price = 5_490_000m, Quantity = 8 }
    ];

    private static int _nextId = 4;

    [HttpGet]
    public ActionResult<IEnumerable<Product>> GetAll()
    {
        return Ok(Products);
    }

    [HttpGet("{id:int}")]
    public ActionResult<Product> GetById(int id)
    {
        var product = Products.FirstOrDefault(p => p.Id == id);

        if (product is null)
            return NotFound();

        return Ok(product);
    }

    [HttpPost]
    public ActionResult<Product> Create(Product product)
    {
        if (string.IsNullOrWhiteSpace(product.Name)
            || product.Price < 0
            || product.Quantity < 0)
        {
            return BadRequest("Invalid product information.");
        }

        product.Id = _nextId++;
        Products.Add(product);

        return CreatedAtAction(
            nameof(GetById),
            new { id = product.Id },
            product);
    }

    [HttpPut("{id:int}")]
    public IActionResult Update(int id, Product input)
    {
        var product = Products.FirstOrDefault(p => p.Id == id);

        if (product is null)
            return NotFound();

        if (string.IsNullOrWhiteSpace(input.Name)
            || input.Price < 0
            || input.Quantity < 0)
        {
            return BadRequest("Invalid product information.");
        }

        product.Name = input.Name;
        product.Price = input.Price;
        product.Quantity = input.Quantity;

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        var product = Products.FirstOrDefault(p => p.Id == id);

        if (product is null)
            return NotFound();

        Products.Remove(product);

        return NoContent();
    }
}
