using KRTBanking.Application.DTOs.Customer;
using KRTBanking.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace KRTBanking.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LimitController : ControllerBase
{
    private readonly ICustomerService _customerService;
    private readonly ILogger<CustomerController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="CustomerController"/> class.
    /// </summary>
    /// <param name="customerService">The customer service.</param>
    /// <param name="logger">The logger.</param>
    public LimitController(
        ICustomerService customerService,
        ILogger<CustomerController> logger)
    {
        _customerService = customerService ?? throw new ArgumentNullException(nameof(customerService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
    
    /// <summary>
    /// Adjusts a customer's limit by adding a new limit entry.
    /// </summary>
    /// <param name="id">The customer ID.</param>
    /// <param name="adjustLimitDto">The limit adjustment request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The updated customer.</returns>
    [HttpPost("{id:guid}")]
    [ProducesResponseType(typeof(CustomerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CustomerDto>> AdjustCustomerLimitAsync(
        Guid id,
        [FromBody] AdjustLimitDto adjustLimitDto,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var customer = await _customerService.AdjustCustomerLimitAsync(id, adjustLimitDto, cancellationToken);
            return Ok(customer);
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

}