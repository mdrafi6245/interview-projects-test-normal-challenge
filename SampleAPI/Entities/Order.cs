using System.ComponentModel.DataAnnotations;

namespace SampleAPI.Entities
{
    public class Order
    {
        public int Id { get; set; }
        public DateTime EntryDate { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsInvoiced { get; set; } = true;
        public bool IsDeleted { get; set; } = false;
    }
}
