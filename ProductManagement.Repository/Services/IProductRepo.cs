using ProductManagement.Repository.Models.Domain;

namespace ProductManagement.Repository.Services;

public interface IProductRepo
{
    Task<IEnumerable<Product>> GetAllAsync(bool onlyActive = true, string transactionId = "", CancellationToken cancellationToken = default);
    Task<Product?> GetByIdAsync(Guid id, string transactionId = "", CancellationToken cancellationToken = default);
    Task<Product> CreateAsync(Product product, string transactionId = "", CancellationToken cancellationToken = default);
    Task<Product> UpdateAsync(Product product, string transactionId = "", CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, string transactionId = "", CancellationToken cancellationToken = default);
}
