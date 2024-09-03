using SampleAPI.Entities;
using SampleAPI.Requests;

namespace SampleAPI.Repositories
{
    public interface IOrderRepository
    {
        Task<IEnumerable<Order>> GetRecentOrdersAsync();

        Task<IEnumerable<Order>> GetSpecificOrderAsync(int id);

        Task AddNewOrderAsync(Order order);

        Task<IEnumerable<Order>> GetOrdersAfterDays(int days);
    }
}
