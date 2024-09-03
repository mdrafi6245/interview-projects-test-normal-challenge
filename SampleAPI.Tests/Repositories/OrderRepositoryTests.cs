using FluentAssertions;
using FluentAssertions.Extensions;
using Microsoft.EntityFrameworkCore;
using SampleAPI.Entities;
using SampleAPI.Repositories;
using SampleAPI.Requests;


namespace SampleAPI.Tests.Repositories
{
    public class OrderRepositoryTests
    {
        private readonly SampleApiDbContext _context;
        private readonly OrderRepository _repository;

        public OrderRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<SampleApiDbContext>()
                .UseInMemoryDatabase(databaseName: "OrderTestDb")
                .Options;

            _context = new SampleApiDbContext(options);
            _repository = new OrderRepository(_context);
        }

        [Fact]
        public async Task GetRecentOrdersAsync_ShouldExcludeDeletedOrders()
        {
            // Arrange
            var deletedOrder = new Order
            {
                Id = 3,
                Name = "Deleted Order",
                Description = "A deleted order",
                EntryDate = DateTime.UtcNow,
                IsDeleted = true
            };

            _context.Orders.Add(deletedOrder);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetRecentOrdersAsync();

            // Assert
            //result.Should().BeEmpty();
            result.Should().NotBeNull();
            //result.Count().Should().Be(2);
            result.First().Name.Should().NotBe("Deleted Order");
        }

        [Fact]
        public async Task GetRecentOrdersAsync_ShouldReturnRecentOrders()
        {
            // Arrange
            var recentOrder = new Order
            {
                Id = 1,
                Name = "Recent Order",
                Description = "A recent order",
                EntryDate = DateTime.UtcNow,
                IsDeleted = false
            };

            var oldOrder = new Order
            {
                Id = 2,
                Name = "Old Order",
                Description = "An old order",
                EntryDate = DateTime.UtcNow.AddDays(-2),
                IsDeleted = false
            };

            _context.Orders.Add(recentOrder);
            _context.Orders.Add(oldOrder);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetRecentOrdersAsync();

            // Assert
            result.Should().NotBeNull();
            result.Count().Should().Be(2);
            result.First().Name.Should().Be("Recent Order");
        }        

        [Fact]
        public async Task AddOrderAsync_ShouldAddOrderToDatabase()
        {
            // Arrange
            var newOrder = new Order
            {
                Id = 4,
                Name = "New Order",
                Description = "New Order Description",
                EntryDate = DateTime.UtcNow
            };

            // Act
            await _repository.AddNewOrderAsync(newOrder);

            // Assert
            var orders = await _context.Orders.ToListAsync();
            orders.Should().ContainSingle();
            orders.First().Name.Should().Be("New Order");
        }

        // Cleanup
        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}