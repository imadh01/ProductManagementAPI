using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProductManagement.Repository.Data;
using ProductManagement.Repository.Models.Domain;

namespace ProductManagement.Repository.Services;

public class ProductRepo : IProductRepo
{
    private readonly AppDbContext _context;
    private readonly ILogger<ProductRepo> _logger;

    public ProductRepo(AppDbContext context, ILogger<ProductRepo> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<Product>> GetAllAsync(bool onlyActive = true, string transactionId = "", CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "********************************************\n" +
            "ProductRepo :: GetAllAsync :: Querying products\n" +
            "OnlyActive: {OnlyActive}\n" +
            "TransactionId: {TransactionId}\n" +
            "********************************************",
            onlyActive, transactionId);

        var query = _context.Products.AsQueryable();

        if (onlyActive)
            query = query.Where(p => p.IsActive);

        var products = await query.OrderByDescending(p => p.CreatedDate).ToListAsync(cancellationToken);

        _logger.LogInformation(
            "********************************************\n" +
            "ProductRepo :: GetAllAsync :: SUCCESS\n" +
            "Count: {Count}\n" +
            "TransactionId: {TransactionId}\n" +
            "********************************************",
            products.Count, transactionId);

        return products;
    }

    public async Task<Product?> GetByIdAsync(Guid id, string transactionId = "", CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "********************************************\n" +
            "ProductRepo :: GetByIdAsync :: Querying product\n" +
            "ProductId: {ProductId}\n" +
            "TransactionId: {TransactionId}\n" +
            "********************************************",
            id, transactionId);

        var product = await _context.Products.FirstOrDefaultAsync(p => p.ProductId == id, cancellationToken);

        _logger.LogInformation(
            "********************************************\n" +
            "ProductRepo :: GetByIdAsync :: {StatusLabel}\n" +
            "ProductId: {ProductId}\n" +
            "TransactionId: {TransactionId}\n" +
            "********************************************",
            product != null ? "SUCCESS :: Product found" : "NOT FOUND",
            id, transactionId);

        return product;
    }

    public async Task<Product> CreateAsync(Product product, string transactionId = "", CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "********************************************\n" +
            "ProductRepo :: CreateAsync :: Inserting product\n" +
            "ProductId: {ProductId} | Name: {Name}\n" +
            "TransactionId: {TransactionId}\n" +
            "********************************************",
            product.ProductId, product.Name, transactionId);

        _context.Products.Add(product);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "********************************************\n" +
            "ProductRepo :: CreateAsync :: SUCCESS\n" +
            "ProductId: {ProductId}\n" +
            "TransactionId: {TransactionId}\n" +
            "********************************************",
            product.ProductId, transactionId);

        return product;
    }

    public async Task<Product> UpdateAsync(Product product, string transactionId = "", CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "********************************************\n" +
            "ProductRepo :: UpdateAsync :: Updating product\n" +
            "ProductId: {ProductId}\n" +
            "TransactionId: {TransactionId}\n" +
            "********************************************",
            product.ProductId, transactionId);

        _context.Products.Update(product);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "********************************************\n" +
            "ProductRepo :: UpdateAsync :: SUCCESS\n" +
            "ProductId: {ProductId}\n" +
            "TransactionId: {TransactionId}\n" +
            "********************************************",
            product.ProductId, transactionId);

        return product;
    }

    public async Task<bool> DeleteAsync(Guid id, string transactionId = "", CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "********************************************\n" +
            "ProductRepo :: DeleteAsync :: Soft-deleting product\n" +
            "ProductId: {ProductId}\n" +
            "TransactionId: {TransactionId}\n" +
            "********************************************",
            id, transactionId);

        var product = await _context.Products.FirstOrDefaultAsync(p => p.ProductId == id, cancellationToken);

        if (product == null)
        {
            _logger.LogInformation(
                "********************************************\n" +
                "ProductRepo :: DeleteAsync :: NOT FOUND\n" +
                "ProductId: {ProductId}\n" +
                "TransactionId: {TransactionId}\n" +
                "********************************************",
                id, transactionId);
            return false;
        }

        product.IsActive = false;
        product.UpdatedDate = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "********************************************\n" +
            "ProductRepo :: DeleteAsync :: SUCCESS\n" +
            "ProductId: {ProductId}\n" +
            "TransactionId: {TransactionId}\n" +
            "********************************************",
            id, transactionId);

        return true;
    }
}