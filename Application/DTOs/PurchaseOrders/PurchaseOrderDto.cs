using Domain.Enums;

namespace Application.DTOs.PurchaseOrders
{
    public class PurchaseOrderDto
    {
        public int Id { get; set; }
        public string PurchaseOrderCode { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public int SupplierId { get; set; }
        public string? SupplierName { get; set; }
        public bool HasVAT { get; set; }
        public PurchaseOrderStatus Status { get; set; }

        // --- الخصائص اللي كانت ناقصة وسببت الخطأ ---
        public DateTime? SignedAt { get; set; }
        public string? SignedByUserId { get; set; }
        // ------------------------------------------

        public List<PurchaseOrderItemDto> Items { get; set; } = new();

        public decimal TotalAmount
        {
            get
            {
                // استخدام ?. لتجنب الـ Null Reference لو القائمة فاضية
                var subtotal = Items?.Sum(x => x.Total) ?? 0;
                return subtotal * (HasVAT ? 1.05m : 1.00m);
            }
            set
            {
                // الـ Setter فارغ لأننا بنحسب القيمة تلقائي، 
                // بس وجوده مهم عشان الـ Binding ميزعلش
            }
        }
    }
}
