using KRTBanking.Application.DTOs.Customer;
using KRTBanking.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace KRTBanking.API.Controllers;

/// <summary>
/// Controller for managing customer operations.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class CustomerController : ControllerBase
{
    private readonly ICustomerService _customerService;

    /// <summary>
    /// Initializes a new instance of the <see cref="CustomerController"/> class.
    /// </summary>
    /// <param name="customerService">The customer service.</param>
    /// <param name="logger">The logger.</param>
    public CustomerController(
        ICustomerService customerService,
        ILogger<CustomerController> logger)
    {
        _customerService = customerService ?? throw new ArgumentNullException(nameof(customerService));
    }

    /// <summary>
    /// Creates a new customer.
    /// </summary>
    /// <param name="createCustomerDto">The create customer request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The created customer.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(CustomerDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CustomerDto>> CreateCustomerAsync(
        [FromBody] CreateCustomerDto createCustomerDto,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var customer = await _customerService.CreateCustomerAsync(createCustomerDto, cancellationToken);
            
            return CreatedAtRoute("GetCustomerById", new { id = customer.Id }, customer);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("already exists"))
        {
            return Conflict(new ProblemDetails
            {
                Title = "Customer Already Exists",
                Detail = ex.Message,
                Status = StatusCodes.Status409Conflict
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid Request",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
    }

    /// <summary>
    /// Gets a customer by ID.
    /// </summary>
    /// <param name="id">The customer ID.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The customer if found.</returns>
    [HttpGet("{id:guid}", Name = "GetCustomerById")]
    [ProducesResponseType(typeof(CustomerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CustomerDto>> GetCustomerByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var customer = await _customerService.GetCustomerByIdAsync(id, cancellationToken);
        
        if (customer is null)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Customer Not Found",
                Detail = $"Customer with ID {id} was not found.",
                Status = StatusCodes.Status404NotFound
            });
        }

        return Ok(customer);
    }

    /// <summary>
    /// Gets a paginated list of customers.
    /// </summary>
    /// <param name="pageSize">The page size (1-100).</param>
    /// <param name="lastEvaluatedKey">The pagination key for the next page.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A paginated list of customers.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PagedCustomersDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedCustomersDto>> GetCustomersAsync(
        [FromQuery] int pageSize = 10,
        [FromQuery] string? lastEvaluatedKey = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var pagedCustomers = await _customerService.GetCustomersAsync(pageSize, lastEvaluatedKey, cancellationToken);
            return Ok(pagedCustomers);
        }
        catch (ArgumentOutOfRangeException ex)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid Page Size",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid Request",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
    }
    
    /// <summary>
    /// Deletes a customer (soft delete).
    /// The customer data is retained for compliance but the customer cannot perform banking operations.
    /// This endpoint provides DELETE semantics while performing deactivation in the domain.
    /// </summary>
    /// <param name="id">The customer ID.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>No content if successful.</returns>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DeleteCustomerAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var deactivateDto = new DeactivateCustomerDto
            {
                Reason = "Customer deletion requested via API",
                DeactivatedBy = "API User"
            };

            await _customerService.DeactivateCustomerAsync(id, deactivateDto, cancellationToken);
            return NoContent();
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            return NotFound(new ProblemDetails
            {
                Title = "Customer Not Found",
                Detail = ex.Message,
                Status = StatusCodes.Status404NotFound
            });
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("already inactive"))
        {
            return Conflict(new ProblemDetails
            {
                Title = "Customer Already Deleted",
                Detail = "The customer has already been deleted.",
                Status = StatusCodes.Status409Conflict
            });
        }
    }
}
