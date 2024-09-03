using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using SampleAPI.Controllers;
using SampleAPI.Entities;
using SampleAPI.Repositories;
using SampleAPI.Requests;


namespace SampleAPI.Tests.Controllers
{
    public class OrdersControllerTests
    {
        private readonly OrdersController _controller;
        private readonly Mock<IOrderRepository> _mockOrderRepository;
        private readonly Mock<ILogger<OrdersController>> _mockLogger;

        public OrdersControllerTests()
        {
            _mockOrderRepository = new Mock<IOrderRepository>();
            _mockLogger = new Mock<ILogger<OrdersController>>();
            _controller = new OrdersController(_mockOrderRepository.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task GetRecentOrders_ShouldReturnRecentOrders()
        {
            // Arrange
            var recentOrders = new List<Order>
            {
                new Order { Id = 1, Name = "Order1", Description = "Description1", EntryDate = DateTime.UtcNow.AddHours(-1) },
                new Order { Id = 2, Name = "Order2", Description = "Description2", EntryDate = DateTime.UtcNow.AddHours(-2) }
            };

            _mockOrderRepository.Setup(repo => repo.GetRecentOrdersAsync())
                .ReturnsAsync(recentOrders);

            // Act
            var result = await _controller.GetRecentOrders() as OkObjectResult;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(StatusCodes.Status200OK);
            var orders = result.Value as List<Order>;
            orders.Should().HaveCount(2);
            orders.Should().Contain(o => o.Id == 1 && o.Name == "Order1");
        }

        [Fact]
        public async Task GetRecentOrders_ShouldReturnNotFound_WhenNoRecentOrders()
        {
            // Arrange
            _mockOrderRepository.Setup(repo => repo.GetRecentOrdersAsync())
                .ReturnsAsync(new List<Order>());

            // Act
            var result = await _controller.GetRecentOrders() as NotFoundObjectResult;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(StatusCodes.Status404NotFound);
            result.Value.Should().Be("No recent orders found.");
        }

        [Fact]
        public async Task GetSpecficOrder_ShouldReturnSpecificOrder()
        {
            // Arrange
            var order = new List<Order>
            {
                new Order { Id = 1, Name = "Order1", Description = "Description1", EntryDate = DateTime.UtcNow.AddHours(-1) }
            };
            int id = 1;

            _mockOrderRepository.Setup(repo => repo.GetSpecificOrderAsync(id))
                .ReturnsAsync(order);

            // Act
            var result = await _controller.GetOrderById(id) as OkObjectResult;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(StatusCodes.Status200OK);
            var orders = result.Value as List<Order>;
            orders.Should().HaveCount(1);
            orders.Should().Contain(o => o.Id == 1 && o.Name == "Order1");
        }

        [Fact]
        public async Task GetSpecificOrder_ShouldReturnNotFound_WhenNoSpecificOrderExist()
        {
            int id = 10;
            // Arrange
            _mockOrderRepository.Setup(repo => repo.GetSpecificOrderAsync(id))
                .ReturnsAsync(new List<Order>());

            // Act
            var result = await _controller.GetOrderById(id) as NotFoundObjectResult;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(StatusCodes.Status404NotFound);
            result.Value.Should().Be("No order found.");
        }


        [Fact]
        public async Task SubmitOrder_ShouldReturnCreated()
        {
            // Arrange
            var newOrder = new Order { Id = 6, Name = "New Order", Description = "New Order Description", EntryDate = DateTime.UtcNow };

            _mockOrderRepository.Setup(repo => repo.AddNewOrderAsync(newOrder))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.CreateOrder(newOrder) as CreatedAtActionResult;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(StatusCodes.Status201Created);
            result.ActionName.Should().Be("GetRecentOrders");
            result.Value.Should().Be(newOrder);
        }

        [Fact]
        public async Task SubmitOrder_ShouldReturnBadRequest_WhenInvalidOrder()
        {
            // Arrange
            var invalidOrder = new Order { Name = "", Description = "" }; // Invalid order data

            // Act
            var result = await _controller.CreateOrder(invalidOrder) as BadRequestObjectResult;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
            result.Value.Should().Be("Invalid order name or description.");
        }

        [Fact]
        public async Task SubmitOrder_ShouldReturnServerError_WhenExceptionOccurs()
        {
            // Arrange
            var newOrder = new Order { Id = 1, Name = "Order", Description = "Description" };

            _mockOrderRepository.Setup(repo => repo.AddNewOrderAsync(newOrder))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _controller.CreateOrder(newOrder) as ObjectResult;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            result.Value.Should().Be("An error occurred while processing your request.");
        }


        [Fact]
        public async Task GetOrdersBasedOnNumberOfWorkingDays_ShouldReturnOrdersIfExist()
        {
            // Arrange
            var order = new List<Order>
            {
                new Order { Id = 1, Name = "Order1", Description = "Description1", EntryDate = DateTime.UtcNow.AddHours(-1) },
                new Order { Id = 2, Name = "Order2", Description = "Description2", EntryDate = DateTime.UtcNow.AddHours(-1) },
                new Order { Id = 3, Name = "Order3", Description = "Description3", EntryDate = DateTime.UtcNow.AddHours(-1) },
                new Order { Id = 4, Name = "Order4", Description = "Description4", EntryDate = DateTime.UtcNow.AddHours(-1) }
            };
            int days = 5;

            _mockOrderRepository.Setup(repo => repo.GetOrdersAfterDays(days))
                .ReturnsAsync(order);

            // Act
            var result = await _controller.GetOrdersAfterDays(days) as OkObjectResult;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(StatusCodes.Status200OK);
            var orders = result.Value as List<Order>;
            orders.Should().HaveCount(4);
            orders.Should().Contain(o => o.Id == 1 && o.Name == "Order1");
        }

        [Fact]
        public async Task GetOrdersBasedOnNumberOfWorkingDays_ShouldReturnNotFound_WhenNoOrdersExist()
        {
            int days = 2;

            // Arrange
            _mockOrderRepository.Setup(repo => repo.GetOrdersAfterDays(days))
                .ReturnsAsync(new List<Order>());

            // Act
            var result = await _controller.GetOrdersAfterDays(days) as NotFoundObjectResult;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(StatusCodes.Status404NotFound);
            result.Value.Should().Be("No orders found.");
        }

    }
}
