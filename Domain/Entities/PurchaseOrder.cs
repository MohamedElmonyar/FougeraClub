using Domain.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    public class PurchaseOrder
    {
        public int Id { get; set; }
        public string PurchaseOrderCode { get; set; } = string.Empty;
        public DateTime Date { get; set; } = DateTime.Now;
        public int SupplierId { get; set; } 
        public bool HasVAT { get; set; }
        public PurchaseOrderStatus Status { get; set; } = PurchaseOrderStatus.Draft;
        public string? SignedByUserId { get; set; } 
        public DateTime? SignedAt { get; set; }
        public ICollection<PurchaseOrderItem> Items { get; set; } = new List<PurchaseOrderItem>();
        [NotMapped]
        public decimal TotalAmount => Items.Sum(x => x.Total) * (HasVAT ? 1.05m : 1.00m);
    }
}
