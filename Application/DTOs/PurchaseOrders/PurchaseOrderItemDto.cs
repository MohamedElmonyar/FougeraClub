using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs.PurchaseOrders
{
    public class PurchaseOrderItemDto
    {
        public int Id { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public int Quantity { get; set; } = 1;
        public decimal SinglePrice { get; set; }
        public int PurchaseOrderId { get; set; }
        public decimal Total => Quantity * SinglePrice;
    }
}
