using Microsoft.AspNetCore.Mvc;
using SampleAPI.Entities;
using SampleAPI.Repositories;

namespace SampleAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ILogger<OrdersController> _logger;

        public OrdersController(IOrderRepository orderRepository, ILogger<OrdersController> logger)
        {
            _orderRepository = orderRepository;
            _logger = logger;
        }

        // GET: api/orders/recent
        [HttpGet("recent")]
        [ProducesResponseType(StatusCodes.Status200OK)] // Success
        [ProducesResponseType(StatusCodes.Status404NotFound)] // No recent orders found
        [ProducesResponseType(StatusCodes.Status500InternalServerError)] // Server error
        public async Task<IActionResult> GetRecentOrders()
        {
            try
            {
                var recentOrders = await _orderRepository.GetRecentOrdersAsync();

                if (!recentOrders.Any())
                    return NotFound("No recent orders found.");

                return Ok(recentOrders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching recent orders.");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
            }
        }

        // GET: api/orders/specificOrder
        // GET specific order if exist else return empty 
        [HttpGet("specificOrder")]
        [ProducesResponseType(StatusCodes.Status200OK)] // Success
        [ProducesResponseType(StatusCodes.Status404NotFound)] // No order found
        [ProducesResponseType(StatusCodes.Status500InternalServerError)] // Server error
        public async Task<IActionResult> GetOrderById(int id)
        {
            try
            {
                var order = await _orderRepository.GetSpecificOrderAsync(id);

                if (!order.Any())
                    return NotFound("No order found.");

                return Ok(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching specific order.");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
            }
        }

        // POST: api/orders
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)] // Successfully created
        [ProducesResponseType(StatusCodes.Status400BadRequest)] // Invalid input
        [ProducesResponseType(StatusCodes.Status500InternalServerError)] // Server error
        public async Task<IActionResult> CreateOrder([FromBody] Order order)
        {
            // Validating parameters
            if (string.IsNullOrWhiteSpace(order.Name) || order.Name.Length > 100 || string.IsNullOrWhiteSpace(order.Description) || order.Description.Length > 100)
            {
                return BadRequest("Invalid order name or description.");
            }            

            try
            {
                //order.EntryDate = DateTime.UtcNow;
                order.EntryDate = order.EntryDate;
                await _orderRepository.AddNewOrderAsync(order);               

                return CreatedAtAction(nameof(GetRecentOrders), new { id = order.Id }, order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while submitting an order.");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
            }
        }

        // GET: api/orders/ordersBasedOnNumberOfWorkingDays
        // GET orders based on the number of working days
        [HttpGet("ordersBasedOnNumberOfWorkingDays/{days:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)] // Success
        [ProducesResponseType(StatusCodes.Status404NotFound)] // No orders found
        [ProducesResponseType(StatusCodes.Status500InternalServerError)] // Server error
        public async Task<IActionResult> GetOrdersAfterDays(int days)
        {
            try
            {
                var orders = await _orderRepository.GetOrdersAfterDays(days);
                if (!orders.Any())
                    return NotFound("No orders found.");

                return Ok(orders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching orders.");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
            }
        }


    }

}
