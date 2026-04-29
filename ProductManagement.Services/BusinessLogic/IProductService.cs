using ProductManagement.Services.DTOs;

namespace ProductManagement.Services.BusinessLogic;

public interface IProductService
{
    Task<IEnumerable<ProductDTO>> GetAllAsync(bool onlyActive, string transactionId, CancellationToken cancellationToken = default);
    Task<ProductDTO> GetByIdAsync(Guid id, string transactionId, CancellationToken cancellationToken = default);
    Task<ProductDTO> CreateAsync(CreateProductDTO dto, string transactionId, CancellationToken cancellationToken = default);
    Task<ProductDTO> UpdateAsync(Guid id, UpdateProductDTO dto, string transactionId, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, string transactionId, CancellationToken cancellationToken = default);
}