using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Domain.Entities
{
    public class PurchaseOrderItem
    {
        public int Id { get; set; }
        public int Quantity { get; set; }
        public decimal SinglePrice { get; set; }
        public string ItemName { get; set; } = string.Empty;

        // Relationship
        public int PurchaseOrderId { get; set; }
        [ForeignKey(nameof(PurchaseOrderId))]
        public PurchaseOrder PurchaseOrder { get; set; } = null!;

        [NotMapped]
        public decimal Total => Quantity * SinglePrice;
    }
}
