using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductManagement.API.Models.Responses;
using ProductManagement.Services.BusinessLogic;
using ProductManagement.Services.DTOs;

namespace ProductManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize]
[AllowAnonymous] // Remove when authentication is enforced
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly IValidator<CreateProductDTO> _createValidator;
    private readonly IValidator<UpdateProductDTO> _updateValidator;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(
        IProductService productService,
        IValidator<CreateProductDTO> createValidator,
        IValidator<UpdateProductDTO> updateValidator,
        ILogger<ProductsController> logger)
    {
        _productService = productService;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _logger = logger;
    }

    /// <summary>Retrieves all products, optionally filtering by active status.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ProductDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAll(
        [FromQuery] bool onlyActive = true,
        CancellationToken cancellationToken = default)
    {
        var transactionId = $"{DateTime.Now:yyyyMMddHHmmss}-{Guid.NewGuid():N}"[..32];

        _logger.LogInformation(
            "********************************************\n" +
            "ProductsController :: GetAll :: Request received\n" +
            "OnlyActive: {OnlyActive}\n" +
            "TransactionId: {TransactionId}\n" +
            "********************************************",
            onlyActive, transactionId);

        try
        {
            var products = await _productService.GetAllAsync(onlyActive, transactionId, cancellationToken);

            _logger.LogInformation(
                "********************************************\n" +
                "ProductsController :: GetAll :: SUCCESS\n" +
                "Count: {Count}\n" +
                "TransactionId: {TransactionId}\n" +
                "********************************************",
                products.Count(), transactionId);

            return Ok(products);
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex,
                "********************************************\n" +
                "ProductsController :: GetAll :: FAILED :: Request timed out\n" +
                "TransactionId: {TransactionId}\n" +
                "********************************************",
                transactionId);

            return StatusCode(504, new ErrorResponse
            {
                Errors = new List<ErrorDetail>
                {
                    new() { Type = "TIMEOUT_ERROR", Code = "9999998", Message = "The database request timed out." }
                }
            });
        }
    }

    /// <summary>Retrieves a single product by its unique identifier.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ProductDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetById(
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        var transactionId = $"{DateTime.Now:yyyyMMddHHmmss}-{Guid.NewGuid():N}"[..32];

        _logger.LogInformation(
            "********************************************\n" +
            "ProductsController :: GetById :: Request received\n" +
            "ProductId: {ProductId}\n" +
            "TransactionId: {TransactionId}\n" +
            "********************************************",
            id, transactionId);

        try
        {
            var product = await _productService.GetByIdAsync(id, transactionId, cancellationToken);

            _logger.LogInformation(
                "********************************************\n" +
                "ProductsController :: GetById :: SUCCESS\n" +
                "ProductId: {ProductId}\n" +
                "TransactionId: {TransactionId}\n" +
                "********************************************",
                id, transactionId);

            return Ok(product);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogError(ex,
                "********************************************\n" +
                "ProductsController :: GetById :: FAILED :: Product not found\n" +
                "ProductId: {ProductId}\n" +
                "TransactionId: {TransactionId}\n" +
                "********************************************",
                id, transactionId);

            return NotFound(new ErrorResponse
            {
                Errors = new List<ErrorDetail>
                {
                    new() { Type = "NOT_FOUND_ERROR", Code = "1900001", Message = ex.Message }
                }
            });
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex,
                "********************************************\n" +
                "ProductsController :: GetById :: FAILED :: Request timed out\n" +
                "TransactionId: {TransactionId}\n" +
                "********************************************",
                transactionId);

            return StatusCode(504, new ErrorResponse
            {
                Errors = new List<ErrorDetail>
                {
                    new() { Type = "TIMEOUT_ERROR", Code = "9999998", Message = "The database request timed out." }
                }
            });
        }
    }

    /// <summary>Creates a new product.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ProductDTO), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Create(
        [FromBody] CreateProductDTO dto,
        CancellationToken cancellationToken = default)
    {
        var transactionId = $"{DateTime.Now:yyyyMMddHHmmss}-{Guid.NewGuid():N}"[..32];

        _logger.LogInformation(
            "********************************************\n" +
            "ProductsController :: Create :: Request received\n" +
            "TransactionId: {TransactionId}\n" +
            "********************************************",
            transactionId);

        var validationResult = await _createValidator.ValidateAsync(dto, cancellationToken);
        if (!validationResult.IsValid)
        {
            _logger.LogWarning(
                "********************************************\n" +
                "ProductsController :: Create :: FAILED :: Validation error\n" +
                "Errors: {Errors}\n" +
                "TransactionId: {TransactionId}\n" +
                "********************************************",
                string.Join(" | ", validationResult.Errors.Select(e => e.ErrorMessage)),
                transactionId);

            return BadRequest(new ErrorResponse
            {
                Errors = validationResult.Errors.Select(e => new ErrorDetail
                {
                    Type = "VALIDATION_ERROR",
                    Code = "1900004",
                    Message = e.ErrorMessage
                }).ToList()
            });
        }

        try
        {
            var createdProduct = await _productService.CreateAsync(dto, transactionId, cancellationToken);

            _logger.LogInformation(
                "********************************************\n" +
                "ProductsController :: Create :: SUCCESS\n" +
                "ProductId: {ProductId}\n" +
                "TransactionId: {TransactionId}\n" +
                "********************************************",
                createdProduct.ProductId, transactionId);

            return CreatedAtAction(nameof(GetById), new { id = createdProduct.ProductId }, createdProduct);
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex,
                "********************************************\n" +
                "ProductsController :: Create :: FAILED :: Request timed out\n" +
                "TransactionId: {TransactionId}\n" +
                "********************************************",
                transactionId);

            return StatusCode(504, new ErrorResponse
            {
                Errors = new List<ErrorDetail>
                {
                    new() { Type = "TIMEOUT_ERROR", Code = "9999998", Message = "The database request timed out." }
                }
            });
        }
    }

    /// <summary>Updates an existing product.</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ProductDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Update(
        [FromRoute] Guid id,
        [FromBody] UpdateProductDTO dto,
        CancellationToken cancellationToken = default)
    {
        var transactionId = $"{DateTime.Now:yyyyMMddHHmmss}-{Guid.NewGuid():N}"[..32];

        _logger.LogInformation(
            "********************************************\n" +
            "ProductsController :: Update :: Request received\n" +
            "ProductId: {ProductId}\n" +
            "TransactionId: {TransactionId}\n" +
            "********************************************",
            id, transactionId);

        var validationResult = await _updateValidator.ValidateAsync(dto, cancellationToken);
        if (!validationResult.IsValid)
        {
            _logger.LogWarning(
                "********************************************\n" +
                "ProductsController :: Update :: FAILED :: Validation error\n" +
                "Errors: {Errors}\n" +
                "TransactionId: {TransactionId}\n" +
                "********************************************",
                string.Join(" | ", validationResult.Errors.Select(e => e.ErrorMessage)),
                transactionId);

            return BadRequest(new ErrorResponse
            {
                Errors = validationResult.Errors.Select(e => new ErrorDetail
                {
                    Type = "VALIDATION_ERROR",
                    Code = "1900004",
                    Message = e.ErrorMessage
                }).ToList()
            });
        }

        try
        {
            var updatedProduct = await _productService.UpdateAsync(id, dto, transactionId, cancellationToken);

            _logger.LogInformation(
                "********************************************\n" +
                "ProductsController :: Update :: SUCCESS\n" +
                "ProductId: {ProductId}\n" +
                "TransactionId: {TransactionId}\n" +
                "********************************************",
                id, transactionId);

            return Ok(updatedProduct);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogError(ex,
                "********************************************\n" +
                "ProductsController :: Update :: FAILED :: Product not found\n" +
                "ProductId: {ProductId}\n" +
                "TransactionId: {TransactionId}\n" +
                "********************************************",
                id, transactionId);

            return NotFound(new ErrorResponse
            {
                Errors = new List<ErrorDetail>
                {
                    new() { Type = "NOT_FOUND_ERROR", Code = "1900001", Message = ex.Message }
                }
            });
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex,
                "********************************************\n" +
                "ProductsController :: Update :: FAILED :: Request timed out\n" +
                "TransactionId: {TransactionId}\n" +
                "********************************************",
                transactionId);

            return StatusCode(504, new ErrorResponse
            {
                Errors = new List<ErrorDetail>
                {
                    new() { Type = "TIMEOUT_ERROR", Code = "9999998", Message = "The database request timed out." }
                }
            });
        }
    }

    /// <summary>Soft-deletes a product by setting IsActive to false.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Delete(
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        var transactionId = $"{DateTime.Now:yyyyMMddHHmmss}-{Guid.NewGuid():N}"[..32];

        _logger.LogInformation(
            "********************************************\n" +
            "ProductsController :: Delete :: Request received\n" +
            "ProductId: {ProductId}\n" +
            "TransactionId: {TransactionId}\n" +
            "********************************************",
            id, transactionId);

        try
        {
            await _productService.DeleteAsync(id, transactionId, cancellationToken);

            _logger.LogInformation(
                "********************************************\n" +
                "ProductsController :: Delete :: SUCCESS :: Product deactivated\n" +
                "ProductId: {ProductId}\n" +
                "TransactionId: {TransactionId}\n" +
                "********************************************",
                id, transactionId);

            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogError(ex,
                "********************************************\n" +
                "ProductsController :: Delete :: FAILED :: Product not found\n" +
                "ProductId: {ProductId}\n" +
                "TransactionId: {TransactionId}\n" +
                "********************************************",
                id, transactionId);

            return NotFound(new ErrorResponse
            {
                Errors = new List<ErrorDetail>
                {
                    new() { Type = "NOT_FOUND_ERROR", Code = "1900001", Message = ex.Message }
                }
            });
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex,
                "********************************************\n" +
                "ProductsController :: Delete :: FAILED :: Request timed out\n" +
                "TransactionId: {TransactionId}\n" +
                "********************************************",
                transactionId);

            return StatusCode(504, new ErrorResponse
            {
                Errors = new List<ErrorDetail>
                {
                    new() { Type = "TIMEOUT_ERROR", Code = "9999998", Message = "The database request timed out." }
                }
            });
        }
    }
}