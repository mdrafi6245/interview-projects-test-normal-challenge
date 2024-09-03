using Microsoft.EntityFrameworkCore;
using SampleAPI.Entities;

namespace SampleAPI.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly SampleApiDbContext _context;
        private readonly List<DateTime> _holidays = new List<DateTime>
        {
            new DateTime(DateTime.Now.Year, 1, 1),  // New Year's Day
            new DateTime(DateTime.Now.Year, 12, 25) // Christmas Day
            // Add more holidays as needed
        };

        public OrderRepository(SampleApiDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Order>> GetRecentOrdersAsync()
        {
            return await _context.Orders
                .Where(o => !o.IsDeleted && o.EntryDate > DateTime.UtcNow.AddDays(-1))
                .OrderByDescending(o => o.EntryDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Order>> GetSpecificOrderAsync(int id)
        {
            return await _context.Orders
                .Where(o => o.Id == id && !o.IsDeleted)
                .OrderByDescending(o => o.EntryDate)
                .ToListAsync();
        }

        public async Task AddNewOrderAsync(Order order)
        {
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Order>> GetOrdersAfterDays(int days)
        {
            var startDate = GetBusinessDaysStartDate(days);
            return await _context.Orders
                .Where(o => o.EntryDate > startDate && !o.IsDeleted)
                .ToListAsync();
        }

        // Logic to calculate the date to consider for fetching details as we are excluding the holidays and weekends
        private DateTime GetBusinessDaysStartDate(int days)
        {
            DateTime currentDate = DateTime.Today;
            int businessDaysCount = 0;

            while (businessDaysCount < days)
            {
                currentDate = currentDate.AddDays(-1);

                if (!IsWeekendOrHoliday(currentDate))
                {
                    businessDaysCount++;
                }
            }

            return currentDate;
        }

        private bool IsWeekendOrHoliday(DateTime date)
        {
            return date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday;
        }

    }
}

