using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using YourNamespace.Models;
using Microsoft.EntityFrameworkCore;
using Namespace;
using PagedList;
using System.Reflection;
using MySql.Data.EntityFrameworkCore.Extensions;
using System.Data.SqlClient;

namespace YourNamespace.Controllers
{
    //[Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly Apicontext _context;

        public ProductController(Apicontext context)
        {
            _context = context;
        }
        [HttpPost("/products")]
        public IActionResult CreateProduct([FromForm] string name, [FromForm] double price, [FromForm] IFormFile image)
        {
            if (image == null || image.Length == 0)
                return BadRequest("Image is required.");

            string imagePath = SaveImage(image);

            var product = new Product
            {
                Name = name,
                Price = price,
                Image = imagePath
            };

            _context.Products.Add(product);
            _context.SaveChanges();

            return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
        }

        [HttpGet("/products")]
        public IActionResult GetAllProducts()
        {
            var products = _context.Products.Include(p => p.CartItems).Include(p => p.OrderItems).ToList();
            return Ok(products);
        }


        [HttpGet("/products/{id}")]
        public IActionResult GetProduct([FromRoute] long id)
        {
            var product = _context.Products.Find(id);

            if (product == null)
                return NotFound("Product not found.");

            return Ok(product);
        }

        [HttpPut("/products/{id}")]
        public IActionResult UpdateProduct(long id, [FromForm] string name, [FromForm] double? price, [FromForm] IFormFile? image)
        {
            var productToUpdate = _context.Products.Find(id);

            if (productToUpdate == null)
                return NotFound("Product not found.");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!string.IsNullOrEmpty(name))
                productToUpdate.Name = name;

            if (price.HasValue)
                productToUpdate.Price = price.Value;

            if (image != null)
            {
                string newImagePath = SaveImage(image);
                DeleteImage(productToUpdate.Image);
                productToUpdate.Image = newImagePath;
            }

            _context.SaveChanges();

            return NoContent();
        }

        [HttpDelete("/products/{id}")]
        public IActionResult DeleteProduct(long id)
        {
            var productToDelete = _context.Products.Find(id);

            if (productToDelete == null)
                return NotFound("Product not found.");

            DeleteImage(productToDelete.Image);
            _context.Products.Remove(productToDelete);
            _context.SaveChanges();

            return NoContent();
        }

        private string SaveImage(IFormFile image)
        {
            string imageName = $"{DateTime.Now.Ticks}_{image.FileName}";
            string imagePath = Path.Combine("uploads", imageName); // Change this to your desired image storage location

            Directory.CreateDirectory(Path.GetDirectoryName(imagePath));
            using (var fileStream = new FileStream(imagePath, FileMode.Create))
            {
                image.CopyTo(fileStream);
            }

            return imageName;
        }

        private void DeleteImage(string imagePath)
        {
            string oldImagePath = Path.Combine("uploads", imagePath);
            if (System.IO.File.Exists(oldImagePath))
                System.IO.File.Delete(oldImagePath);
        }
        [HttpGet("/products/search")]
        // public async Task<IActionResult> GetAllProducts([FromBody] ProductSearchParameters parameters)
        public async Task<IActionResult> GetAllProducts(
            [FromForm] string? keyword,
            [FromForm(Name = "sort")] string? sortField,
            [FromForm(Name = "order")] string? sortOrder,
            [FromForm] int page = 0,
            [FromForm] int size = 10)
        {
            var skip = page * size;
            var limit = size;

            var query = _context.Products.AsQueryable();

            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(product => EF.Functions.Like(product.Name, $"%{keyword}%"));
            }

            switch (sortField)
            {
                case "name":
                    query = sortOrder.ToLower() == "asc"
                        ? query.OrderBy(p => p.Name)
                        : query.OrderByDescending(p => p.Name);
                    break;
                case "price":
                    query = sortOrder.ToLower() == "asc"
                        ? query.OrderBy(p => p.Price)
                        : query.OrderByDescending(p => p.Price);
                    break;
                default:
                    query = sortOrder.ToLower() == "asc"
                        ? query.OrderBy(p => p.Id)
                        : query.OrderByDescending(p => p.Id);
                    break;
            }

            var totalCount = await query.CountAsync();
            var products = await query
                .Skip(skip)
                .Take(limit)
                .ToListAsync();

            var result = new
            {
                Items = products,
                Page = page,
                PageSize = size,
                TotalCount = totalCount
            };

            return Ok(result);
        }
    }
}

