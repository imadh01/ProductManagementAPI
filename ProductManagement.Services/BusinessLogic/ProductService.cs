using AutoMapper;
using Microsoft.Extensions.Logging;
using ProductManagement.Repository.Models.Domain;
using ProductManagement.Repository.Services;
using ProductManagement.Services.DTOs;

namespace ProductManagement.Services.BusinessLogic;

public class ProductService : IProductService
{
    private readonly IProductRepo _productRepo;
    private readonly IMapper _mapper;
    private readonly ILogger<ProductService> _logger;

    public ProductService(IProductRepo productRepo, IMapper mapper, ILogger<ProductService> logger)
    {
        _productRepo = productRepo;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IEnumerable<ProductDTO>> GetAllAsync(bool onlyActive, string transactionId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "********************************************\n" +
            "ProductService :: GetAllAsync :: Retrieving products\n" +
            "OnlyActive: {OnlyActive}\n" +
            "TransactionId: {TransactionId}\n" +
            "********************************************",
            onlyActive, transactionId);

        var products = await _productRepo.GetAllAsync(onlyActive, transactionId, cancellationToken);
        var productDtos = _mapper.Map<IEnumerable<ProductDTO>>(products);

        _logger.LogInformation(
            "********************************************\n" +
            "ProductService :: GetAllAsync :: SUCCESS\n" +
            "Count: {Count}\n" +
            "TransactionId: {TransactionId}\n" +
            "********************************************",
            productDtos.Count(), transactionId);

        return productDtos;
    }

    public async Task<ProductDTO> GetByIdAsync(Guid id, string transactionId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "********************************************\n" +
            "ProductService :: GetByIdAsync :: Retrieving product\n" +
            "ProductId: {ProductId}\n" +
            "TransactionId: {TransactionId}\n" +
            "********************************************",
            id, transactionId);

        var product = await _productRepo.GetByIdAsync(id, transactionId, cancellationToken);

        if (product == null)
        {
            _logger.LogError(
                "********************************************\n" +
                "ProductService :: GetByIdAsync :: FAILED :: Product not found\n" +
                "ProductId: {ProductId}\n" +
                "TransactionId: {TransactionId}\n" +
                "********************************************",
                id, transactionId);

            throw new KeyNotFoundException($"Product with ID '{id}' was not found.");
        }

        _logger.LogInformation(
            "********************************************\n" +
            "ProductService :: GetByIdAsync :: SUCCESS\n" +
            "ProductId: {ProductId}\n" +
            "TransactionId: {TransactionId}\n" +
            "********************************************",
            id, transactionId);

        return _mapper.Map<ProductDTO>(product);
    }

    public async Task<ProductDTO> CreateAsync(CreateProductDTO dto, string transactionId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "********************************************\n" +
            "ProductService :: CreateAsync :: Creating product\n" +
            "Name: {Name} | Price: {Price}\n" +
            "TransactionId: {TransactionId}\n" +
            "********************************************",
            dto.Name, dto.Price, transactionId);

        var product = _mapper.Map<Product>(dto);
        product.ProductId = Guid.NewGuid();
        product.IsActive = true;
        product.CreatedDate = DateTime.UtcNow;

        var createdProduct = await _productRepo.CreateAsync(product, transactionId, cancellationToken);

        _logger.LogInformation(
            "********************************************\n" +
            "ProductService :: CreateAsync :: SUCCESS\n" +
            "ProductId: {ProductId}\n" +
            "TransactionId: {TransactionId}\n" +
            "********************************************",
            createdProduct.ProductId, transactionId);

        return _mapper.Map<ProductDTO>(createdProduct);
    }

    public async Task<ProductDTO> UpdateAsync(Guid id, UpdateProductDTO dto, string transactionId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "********************************************\n" +
            "ProductService :: UpdateAsync :: Updating product\n" +
            "ProductId: {ProductId}\n" +
            "TransactionId: {TransactionId}\n" +
            "********************************************",
            id, transactionId);

        var existingProduct = await _productRepo.GetByIdAsync(id, transactionId, cancellationToken);

        if (existingProduct == null)
        {
            _logger.LogError(
                "********************************************\n" +
                "ProductService :: UpdateAsync :: FAILED :: Product not found\n" +
                "ProductId: {ProductId}\n" +
                "TransactionId: {TransactionId}\n" +
                "********************************************",
                id, transactionId);

            throw new KeyNotFoundException($"Product with ID '{id}' was not found.");
        }

        _mapper.Map(dto, existingProduct);
        existingProduct.ProductId = id;
        existingProduct.UpdatedDate = DateTime.UtcNow;

        var updatedProduct = await _productRepo.UpdateAsync(existingProduct, transactionId, cancellationToken);

        _logger.LogInformation(
            "********************************************\n" +
            "ProductService :: UpdateAsync :: SUCCESS\n" +
            "ProductId: {ProductId}\n" +
            "TransactionId: {TransactionId}\n" +
            "********************************************",
            id, transactionId);

        return _mapper.Map<ProductDTO>(updatedProduct);
    }

    public async Task<bool> DeleteAsync(Guid id, string transactionId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "********************************************\n" +
            "ProductService :: DeleteAsync :: Deleting product\n" +
            "ProductId: {ProductId}\n" +
            "TransactionId: {TransactionId}\n" +
            "********************************************",
            id, transactionId);

        var result = await _productRepo.DeleteAsync(id, transactionId, cancellationToken);

        if (!result)
        {
            _logger.LogError(
                "********************************************\n" +
                "ProductService :: DeleteAsync :: FAILED :: Product not found\n" +
                "ProductId: {ProductId}\n" +
                "TransactionId: {TransactionId}\n" +
                "********************************************",
                id, transactionId);

            throw new KeyNotFoundException($"Product with ID '{id}' was not found.");
        }

        _logger.LogInformation(
            "********************************************\n" +
            "ProductService :: DeleteAsync :: SUCCESS\n" +
            "ProductId: {ProductId}\n" +
            "TransactionId: {TransactionId}\n" +
            "********************************************",
            id, transactionId);

        return true;
    }
}