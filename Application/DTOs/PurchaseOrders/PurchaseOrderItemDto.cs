using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs.PurchaseOrders
{
    public record PurchaseOrderItemDto(
         string ItemName,
         int Quantity,
         decimal SinglePrice
     );
}
