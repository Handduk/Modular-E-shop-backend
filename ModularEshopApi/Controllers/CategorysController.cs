﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ModularEshopApi.Data;
using ModularEshopApi.Dto.Category;
using ModularEshopApi.Dto.Product;
using ModularEshopApi.Models;

namespace ModularEshopApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategorysController : Controller
    {
        private readonly ApiDbContext _context;
        private readonly IWebHostEnvironment _env;

        public CategorysController(ApiDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        private string GetSafeFolderName(string name)
        {
            var invalidChars = Path.GetInvalidFileNameChars();
            var cleaned = new string(name
                .Trim()
                .ToLower()
                .Replace(" ", "-")
                .Where(c => !invalidChars.Contains(c))
                .ToArray());

            return cleaned;
        }

        //GET: api/Categorys/
        [HttpGet]
        public async Task<ActionResult<ActionResult<GetCategoryDTO>>> GetCategorys()
        {
            try
            {
                var baseUrl = $"{Request.Scheme}://{Request.Host}";

                var listOfCategorys = await _context.Categorys.Select(category => new GetCategoryDTO
                {
                    Id = category.Id,
                    Name = category.Name,
                    Description = category.Description,
                    ImageUrl = string.IsNullOrEmpty(category.Image) ? null : $"{baseUrl}/{category.Image}"
                }).ToListAsync();

                return Ok(listOfCategorys);

            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        // GET: api/Categorys/1
        [HttpGet("{id}")]
        public async Task<ActionResult<GetCategoryDTO>> GetCategory(int id)
        {
            try
            {
                var category = await _context.Categorys.FindAsync(id);
                if (category == null)
                {
                    return NotFound();
                }

                var products = await _context.Products.Where(p => p.CategoryId == category.Id).ToListAsync();
                var baseUrl = $"{Request.Scheme}://{Request.Host}";

                var dto = new GetCategoryDTO
                {
                    Id = category.Id,
                    Name = category.Name,
                    Description = category.Description,
                    ImageUrl = string.IsNullOrEmpty(category.Image) ? null : $"{baseUrl}/{category.Image}",
                    Products = [.. products.Select(p => new GetProductsDTO
                    {
                        Id = p.Id,
                        Name = p.Name,
                        Description = p.Description,
                        Brand = p.Brand,
                        CategoryId = p.CategoryId,
                        Discount = p.Discount,
                        Options = p.Options,
                        Price = p.Price,
                        Variants = p.Variants,
                        Images = p.Images?.Select(img =>
{
    if (img.StartsWith("http", StringComparison.OrdinalIgnoreCase))
    {
        // Remove the existing base URL (up to the third slash)
        var uri = new Uri(img);
        var relativePath = uri.PathAndQuery.TrimStart('/');
        return $"{baseUrl}/{relativePath}";
    }
    else
    {
        return $"{baseUrl}/{img.TrimStart('/')}";
    }
}).ToList()
                    })]
                };
                return Ok(dto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        //POST: api/Categorys
        [HttpPost]
        public async Task<ActionResult<Category>> CreateCategory([FromForm] CreateCategoryDTO dto)
        {
            try
            {
                string? imagePath = null;

                var category = new Category
                {
                    Name = dto.Name,
                    Description = dto.Description,
                    Products = new List<Product>()
                };
                _context.Categorys.Add(category);
                await _context.SaveChangesAsync();

                var folderName = $"{GetSafeFolderName(dto.Name)}-{category.Id}";
                var directoryPath = Path.Combine(_env.WebRootPath, "categorys", folderName);

                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                if (dto.Image != null)
                {
                    var fileName = Path.GetRandomFileName() + Path.GetExtension(dto.Image.FileName);
                    var filePath = Path.Combine(directoryPath, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await dto.Image.CopyToAsync(stream);
                    }
                    imagePath = $"categorys/{folderName}/{fileName}";
                }

                category.Image = imagePath;
                _context.Categorys.Update(category);
                await _context.SaveChangesAsync();

                return CreatedAtAction("GetCategory", new { id = category.Id }, category);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        //C:\Users\Marti\repos\ModularEshopApi\ModularEshopApi\wwwroot\categorys\name-id\filename

        //DELETE: api/Categorys/1
        [HttpDelete("{id}")]
        public async Task<ActionResult<Category>> DeleteCategory(int id)
        {
            try
            {
                var category = await _context.Categorys.FindAsync(id);
                if (category == null)
                {
                    return NotFound();
                }
                try
                {
                    var folderName = $"{GetSafeFolderName(category.Name)}-{id}";
                    var dir = Path.Combine(_env.WebRootPath, "categorys", folderName);
                    if (Directory.Exists(dir))
                    {
                        Directory.Delete(dir, recursive: true);
                    }
                }
                catch (Exception ex)
                {
                    return StatusCode(500, "Internal server error: " + ex.Message);
                }
                _context.Categorys.Remove(category);
                await _context.SaveChangesAsync();
                return category;
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        // PATCH: api/Products/1
        [HttpPatch("{id}")]
        public async Task<ActionResult<Category>> UpdateCategory(int id, [FromForm] UpdateCategoryDTO dto)
        {
            try
            {
                var category = await _context.Categorys.FindAsync(id);
                if (category == null)
                {
                    return NotFound("Category not found");
                }
                if (dto.Image != null)
                {
                    //Delete the old image if it exists
                    if (!string.IsNullOrEmpty(category.Image))
                    {
                        var oldImagePath = Path.Combine(_env.WebRootPath, "categorys", dto.Name, category.Image.Replace("/", Path.DirectorySeparatorChar.ToString()));
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }
                    var directoryPath = Path.Combine(_env.WebRootPath, "categorys", dto.Name);
                    var fileName = Path.GetRandomFileName() + Path.GetExtension(dto.Image.FileName);
                    var filePath = Path.Combine(directoryPath, fileName);

                    if (!Directory.Exists(directoryPath))
                    {
                        Directory.CreateDirectory(directoryPath);
                    }

                    using var stream = new FileStream(filePath, FileMode.Create);
                    await dto.Image.CopyToAsync(stream);

                    category.Image = "categorys/images/" + fileName;
                }
                else if (dto.RemoveImage && !string.IsNullOrEmpty(category.Image))
                {
                    //Delete the old image if it exists
                    var oldImagePath = Path.Combine(_env.WebRootPath, "categorys", dto.Name, category.Image.Replace("/", Path.DirectorySeparatorChar.ToString()));
                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                    category.Image = null;
                }
                _context.Categorys.Update(category);
                await _context.SaveChangesAsync();
                return category;
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }
    }
}
