using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ModularEshopApi.Data;
using ModularEshopApi.Dto.Product;
using ModularEshopApi.Models;

namespace ModularEshopApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly ApiDbContext _context;
        private readonly IWebHostEnvironment _env;

        public ProductsController(ApiDbContext context, IWebHostEnvironment env)
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

        //GET: api/Products/
        [HttpGet]
        public async Task<ActionResult<ActionResult<GetProductsDTO>>> GetProducts()
        {
            try
            {
                var baseUrl = $"{Request.Scheme}://{Request.Host}";
                var products = await _context.Products.ToListAsync();
                var listOfProducts = products.Select(product => new GetProductsDTO
                {
                    Id = product.Id,
                    Name = product.Name,
                    Description = product.Description,
                    CategoryId = product.CategoryId,
                    Brand = product.Brand,
                    Options = product.Options,
                    Price = product.Price,
                    Variants = product.Variants,
                    Discount = product.Discount,
                    Images = product.Images?.Select(image => $"{baseUrl}/{image}").ToList()
                }).ToList();
                if (listOfProducts == null)
                {
                    return NotFound();
                }
                return Ok(listOfProducts);

            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }


        // GET: api/Products/1
        [HttpGet("id")]
        public async Task<ActionResult<GetProductsDTO>> GetProduct(int id)
        {
            try
            {
                var product = await _context.Products.FindAsync(id);
                if (product == null)
                {
                    return NotFound();
                }
                var baseUrl = $"{Request.Scheme}://{Request.Host}";

                var dto = new GetProductsDTO
                {
                    Id = product.Id,
                    Name = product.Name,
                    Description = product.Description,
                    CategoryId = product.CategoryId,
                    Brand = product.Brand,
                    Options = product.Options,
                    Price = product.Price,
                    Variants = product.Variants,
                    Discount = product.Discount,
                    Images = product.Images?.Select(image => image.StartsWith("http://") || image.StartsWith("https://")
                    ? image
                    : $"{baseUrl}/{image.TrimStart('/')}").ToList()
                };
                return Ok(dto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        // GET: api/Products/
        [HttpGet("productslist")]
        public async Task<ActionResult<List<GetProductsDTO>>> GetProductList([FromQuery] int[] ids)
        {
            try
            {
                var baseUrl = $"{Request.Scheme}://{Request.Host}";
                var products = await _context.Products.Where(p => ids.Contains(p.Id)).ToListAsync();
                if (products == null || !products.Any())
                {
                    return NotFound("No products found for the provided IDs");
                }
                var listOfProducts = products.Select(product => new GetProductsDTO
                {
                    Id = product.Id,
                    Name = product.Name,
                    Description = product.Description,
                    CategoryId = product.CategoryId,
                    Brand = product.Brand,
                    Options = product.Options,
                    Price = product.Price,
                    Variants = product.Variants,
                    Discount = product.Discount,
                    Images = product.Images?.Select(image => image.StartsWith("http://") || image.StartsWith("https://")
                    ? image
                    : $"{baseUrl}/{image.TrimStart('/')}").ToList()
                });
                return Ok(listOfProducts);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        //POST: api/Products
        [HttpPost]
        public async Task<ActionResult<Product>> CreateProduct([FromForm] CreateProductDTO dto)
        {
            try
            {
                var imagePaths = new List<string>();
                var product = new Product
                {
                    CategoryId = dto.CategoryId,
                    Brand = dto.Brand,
                    Name = dto.Name,
                    Description = dto.Description,
                    Options = dto.Options,
                    Price = dto.Price,
                    Discount = dto.Discount,

                };

                _context.Products.Add(product);
                await _context.SaveChangesAsync();

                if (dto.Images != null && dto.Images.Count > 0)
                {
                    var category = await _context.Categorys.FindAsync(dto.CategoryId);
                    if (category == null)
                    {
                        return NotFound($"Category with ID {dto.CategoryId} not found");
                    }

                    foreach (var file in dto.Images)
                    {
                        var folderName = $"{GetSafeFolderName(dto.Name)}-{product.Id}";
                        var categoryName = $"{GetSafeFolderName(category.Name)}-{dto.CategoryId}";
                        var directoryPath = Path.Combine(_env.WebRootPath, "categorys", categoryName, "products", folderName);
                        if (!Directory.Exists(directoryPath))
                        {
                            Directory.CreateDirectory(directoryPath);
                        }

                        var fileName = Path.GetRandomFileName() + Path.GetExtension(file.FileName);
                        var filePath = Path.Combine(directoryPath, fileName);

                        using var stream = new FileStream(filePath, FileMode.Create);
                        await file.CopyToAsync(stream);

                        imagePaths.Add($"categorys/{categoryName}/products/{folderName}/{fileName}");
                    }
                }
                product.Images = imagePaths;
                if (dto.Variants != null && dto.Variants.Count > 0)
                {
                    product.Variants = dto.Variants;
                }
                else
                {
                    product.Variants = new List<Variant>();
                }
                _context.Products.Update(product);
                await _context.SaveChangesAsync();
                return CreatedAtAction("GetProduct", new { id = product.Id }, product);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message +
                    (ex.InnerException != null ? " | Inner: " + ex.InnerException.Message : ""));
            }
        }


        //DELETE: api/Products/1
        [HttpDelete("{id}")]
        public async Task<ActionResult<Product>> DeleteProduct(int id)
        {
            try
            {
                var product = await _context.Products.FindAsync(id);
                if (product == null)
                {
                    return NotFound();
                }

                try
                {
                    var category = await _context.Categorys.FindAsync(product.CategoryId);
                    if (category == null)
                    {
                        return NotFound();
                    }
                    var folderName = $"{GetSafeFolderName(product.Name)}-{id}";
                    var categoryName = $"{GetSafeFolderName(category.Name)}-{category.Id}"
                    ;
                    var dir = Path.Combine(_env.WebRootPath, "categorys", categoryName, "products", folderName);
                    if (Directory.Exists(dir))
                    {
                        Directory.Delete(dir, recursive: true);
                    }
                }
                catch (Exception ex)
                {
                    return StatusCode(500, "Internal server error: " + ex.Message);
                }
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
                return product;
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        // PATCH: api/Products/1
        [HttpPatch("{id}")]
        public async Task<ActionResult<Product>> UpdateProduct(int id, [FromForm] UpdateProductDTO dto)
        {
            try
            {
                var product = await _context.Products.FindAsync(id);
                if (product == null)
                    return NotFound("Product not found");
                product.Brand = dto.Brand;
                product.Name = dto.Name;
                product.Description = dto.Description;
                product.Options = dto.Options;
                product.Price = dto.Price;
                product.Variants = dto.Variants;
                product.Discount = dto.Discount;


                var existingImages = product.Images ?? new List<string>();
                var existingVariants = product.Variants ?? new List<Variant>();
                var keptVariants = dto.Variants ?? new List<Variant>();
                var keptImages = dto.KeptImages ?? new List<string>();

                var variantsToDelete = existingVariants.Except(keptVariants).ToList();

                var baseUrl = $"{Request.Scheme}://{Request.Host}";
                keptImages = keptImages.Select(image => image.StartsWith(baseUrl) ? image.Replace(baseUrl + "/", "") : image).ToList();

                var imagesToDelete = existingImages.Except(keptImages).ToList();
                var category = await _context.Categorys.FindAsync(product.CategoryId);
                if (category == null)
                {
                    return NotFound($"Category with ID {product.CategoryId} not found");
                }

                var folderName = $"{GetSafeFolderName(dto.Name)}-{product.Id}";
                var categoryName = $"{GetSafeFolderName(category.Name)}-{category.Id}";
                var directoryPath = Path.Combine(_env.WebRootPath, "categorys", categoryName, "products", folderName);


                //Delete old images
                foreach (var image in imagesToDelete)
                {
                    var filePath = Path.Combine(directoryPath, image.Replace("/", Path.DirectorySeparatorChar.ToString()));
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }

                var updatedImages = new List<string>(keptImages);
                //Add new images
                if (dto.NewImages != null)
                {
                    foreach (var image in dto.NewImages)
                    {
                        ;
                        if (!Directory.Exists(directoryPath))
                        {
                            Directory.CreateDirectory(directoryPath);
                        }
                        var fileName = Path.GetRandomFileName() + Path.GetExtension(image.FileName);
                        var filePath = Path.Combine(directoryPath, fileName);


                        using var stream = new FileStream(filePath, FileMode.Create);
                        await image.CopyToAsync(stream);

                        var relativePath = Path.Combine("categorys", categoryName, "products", folderName, fileName).Replace("\\", "/");
                        updatedImages.Add(relativePath);
                    }
                }
                product.Images = updatedImages;
                _context.Products.Update(product);
                await _context.SaveChangesAsync();
                return Ok(product);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

    }
}
