using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ModularEshopApi.Data;
using ModularEshopApi.Dto.Product;
using ModularEshopApi.Models;
using Newtonsoft.Json;

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
        [HttpGet("{id:int}")]
        public async Task<ActionResult<GetProductsDTO>> GetProduct(int id)
        {
            try
            {
                var product = await _context.Products.Include(p => p.Variants)
                                                     .FirstOrDefaultAsync(p => p.Id == id);
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
                var products = await _context.Products
                    .Where(p => ids.Contains(p.Id))
                    .Include(p => p.Variants)
                    .ToListAsync();
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

                var category = await _context.Categorys.FindAsync(dto.CategoryId);
                if (dto.Images != null && dto.Images.Count > 0)
                {
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

                if (!string.IsNullOrEmpty(dto.VariantJson))
                {
                    var variants = JsonConvert.DeserializeObject<List<Variant>>(dto.VariantJson) ?? new List<Variant>();
                    for (int i = 0; i < variants.Count; i++)
                    {
                        var variant = variants[i];
                        variant.ProductId = product.Id;

                        if (dto.VariantImages != null && dto.VariantImages.Count > i)
                        {
                            var variantImage = dto.VariantImages[i];
                            if (variantImage != null)
                            {
                                var folderName = $"{GetSafeFolderName(dto.Name)}-{product.Id}";
                                if (category == null)
                                {
                                    return NotFound($"Category with ID {dto.CategoryId} not found");
                                }
                                var categoryName = $"{GetSafeFolderName(category.Name)}-{dto.CategoryId}";
                                var directoryPath = Path.Combine(_env.WebRootPath, "categorys", categoryName, "products", folderName);
                                if (!Directory.Exists(directoryPath))
                                {
                                    Directory.CreateDirectory(directoryPath);
                                }

                                var fileName = Path.GetRandomFileName() + Path.GetExtension(variantImage.FileName);
                                var filePath = Path.Combine(directoryPath, fileName);

                                using var stream = new FileStream(filePath, FileMode.Create);
                                await variantImage.CopyToAsync(stream);

                                variant.VariantImg = $"categorys/{categoryName}/products/{folderName}/{fileName}";
                            }
                        }
                        _context.Variants.Add(variant);
                    }
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
                List<Variant> incomingVariants = new();
                if (!string.IsNullOrEmpty(dto.VariantJson))
                {
                    incomingVariants = JsonConvert.DeserializeObject<List<Variant>>(dto.VariantJson) ?? new();
                }

                var product = await _context.Products
    .Include(p => p.Variants)
    .FirstOrDefaultAsync(p => p.Id == id);
                if (product == null)
                    return NotFound("Product not found");

                product.Brand = dto.Brand;
                product.Name = dto.Name;
                product.Description = dto.Description;
                product.Options = dto.Options;
                product.Price = dto.Price;
                product.Discount = dto.Discount;

                var existingImages = product.Images ?? new List<string>();
                var keptImages = dto.KeptImages ?? new List<string>();
                var baseUrl = $"{Request.Scheme}://{Request.Host}";
                keptImages = keptImages.Select(img => img.StartsWith(baseUrl) ? img.Replace(baseUrl + "/", "") : img).ToList();
                var imagesToDelete = existingImages.Except(keptImages).ToList();

                var category = await _context.Categorys.FindAsync(product.CategoryId);
                if (category == null)
                    return NotFound($"Category with ID {product.CategoryId} not found");

                var safeCategory = $"{GetSafeFolderName(category.Name)}-{category.Id}";
                var safeProduct = $"{GetSafeFolderName(dto.Name)}-{product.Id}";
                var productPath = Path.Combine(_env.WebRootPath, "categorys", safeCategory, "products", safeProduct);
                if (!Directory.Exists(productPath))
                    Directory.CreateDirectory(productPath);


                foreach (var img in imagesToDelete)
                {
                    var imgPath = Path.Combine(_env.WebRootPath, img.Replace("/", Path.DirectorySeparatorChar.ToString()));
                    if (System.IO.File.Exists(imgPath))
                        System.IO.File.Delete(imgPath);
                }

                var updatedImages = new List<string>(keptImages);
                if (dto.NewImages != null)
                {
                    foreach (var img in dto.NewImages)
                    {
                        var fileName = Path.GetRandomFileName() + Path.GetExtension(img.FileName);
                        var fullPath = Path.Combine(productPath, fileName);

                        using var stream = new FileStream(fullPath, FileMode.Create);
                        await img.CopyToAsync(stream);

                        var relativePath = Path.Combine("categorys", safeCategory, "products", safeProduct, fileName).Replace("\\", "/");
                        updatedImages.Add(relativePath);
                    }
                }
                product.Images = updatedImages;


                var existingVariants = _context.Variants.Where(v => v.ProductId == product.Id).ToList();
                var incomingIds = incomingVariants.Select(v => v.Id).ToHashSet();


                var deletedVariants = existingVariants.Where(ev => !incomingIds.Contains(ev.Id)).ToList();
                foreach (var dv in deletedVariants)
                {
                    if (!string.IsNullOrEmpty(dv.VariantImg))
                    {
                        var oldPath = Path.Combine(_env.WebRootPath, dv.VariantImg.Replace("/", Path.DirectorySeparatorChar.ToString()));
                        if (System.IO.File.Exists(oldPath))
                            System.IO.File.Delete(oldPath);
                    }
                    _context.Variants.Remove(dv);
                }


                var uploadedVariantImages = new Dictionary<int, IFormFile>();
                foreach (var file in dto.VariantImages)
                {
                    var match = Regex.Match(file.FileName, @"variant_(\d+)_");
                    if (match.Success && int.TryParse(match.Groups[1].Value, out int variantId))
                    {
                        uploadedVariantImages[variantId] = file;
                    }
                }

                foreach (var variant in incomingVariants)
                {
                    var existing = existingVariants.FirstOrDefault(v => v.Id == variant.Id);
                    IFormFile? newImage = uploadedVariantImages.GetValueOrDefault(variant.Id);

                    if (existing != null)
                    {
                        existing.VariantName = variant.VariantName;
                        existing.VariantPrice = variant.VariantPrice;

                        if (newImage != null)
                        {
                            // delete old image
                            if (!string.IsNullOrEmpty(existing.VariantImg))
                            {
                                var oldPath = Path.Combine(_env.WebRootPath, existing.VariantImg.Replace("/", Path.DirectorySeparatorChar.ToString()));
                                if (System.IO.File.Exists(oldPath))
                                    System.IO.File.Delete(oldPath);
                            }

                            var fileName = $"variant_{variant.Id}_{Guid.NewGuid()}{Path.GetExtension(newImage.FileName)}";
                            var fullPath = Path.Combine(productPath, fileName);
                            using var stream = new FileStream(fullPath, FileMode.Create);
                            await newImage.CopyToAsync(stream);

                            var relativePath = Path.Combine("categorys", safeCategory, "products", safeProduct, fileName).Replace("\\", "/");
                            existing.VariantImg = relativePath;
                        }

                        _context.Variants.Update(existing);
                    }
                    else
                    {
                        var newVariant = new Variant
                        {
                            ProductId = product.Id,
                            VariantName = variant.VariantName,
                            VariantPrice = variant.VariantPrice
                        };

                        if (newImage != null)
                        {
                            var fileName = $"variant_0_{Guid.NewGuid()}{Path.GetExtension(newImage.FileName)}";
                            var fullPath = Path.Combine(productPath, fileName);
                            using var stream = new FileStream(fullPath, FileMode.Create);
                            await newImage.CopyToAsync(stream);

                            var relativePath = Path.Combine("categorys", safeCategory, "products", safeProduct, fileName).Replace("\\", "/");
                            newVariant.VariantImg = relativePath;
                        }

                        _context.Variants.Add(newVariant);
                    }
                }

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
