using Microsoft.EntityFrameworkCore;
using ProductsAPI.Data;
using ProductsAPI.Models;

namespace ProductsAPI.Services
{
    public class ProductService : IProductService
    {
        private readonly ProductsDbContext _context;
        private readonly ILogger<ProductService> _logger;

        public ProductService(ProductsDbContext context, ILogger<ProductService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<Product>> GetAllProductsAsync()
        {
            _logger.LogInformation("Retrieving all products");
            return await _context.Products.OrderBy(p => p.Name).ToListAsync();
        }

        public async Task<Product?> GetProductByIdAsync(int id)
        {
            _logger.LogInformation("Retrieving product with ID: {ProductId}", id);
            return await _context.Products.FindAsync(id);
        }

        public async Task<Product> CreateProductAsync(Product product)
        {
            _logger.LogInformation("Creating new product: {ProductName}", product.Name);
            
            product.CreatedAt = DateTime.UtcNow;
            product.UpdatedAt = DateTime.UtcNow;
            
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Product created with ID: {ProductId}", product.Id);
            return product;
        }

        public async Task<Product?> UpdateProductAsync(int id, Product product)
        {
            _logger.LogInformation("Updating product with ID: {ProductId}", id);
            
            var existingProduct = await _context.Products.FindAsync(id);
            if (existingProduct == null)
            {
                _logger.LogWarning("Product with ID {ProductId} not found for update", id);
                return null;
            }

            existingProduct.Name = product.Name;
            existingProduct.Description = product.Description;
            existingProduct.Price = product.Price;
            existingProduct.Category = product.Category;
            existingProduct.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Product with ID {ProductId} updated successfully", id);
            return existingProduct;
        }

        public async Task<bool> DeleteProductAsync(int id)
        {
            _logger.LogInformation("Deleting product with ID: {ProductId}", id);
            
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                _logger.LogWarning("Product with ID {ProductId} not found for deletion", id);
                return false;
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Product with ID {ProductId} deleted successfully", id);
            return true;
        }

        public async Task<IEnumerable<Product>> GetProductsByCategoryAsync(string category)
        {
            _logger.LogInformation("Retrieving products by category: {Category}", category);
            return await _context.Products
                .Where(p => p.Category.ToLower() == category.ToLower())
                .OrderBy(p => p.Name)
                .ToListAsync();
        }

        public async Task<int> GetProductCountAsync()
        {
            _logger.LogInformation("Retrieving total product count");
            return await _context.Products.CountAsync();
        }
    }
}