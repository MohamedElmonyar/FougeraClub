

namespace Application.DTOs.PurchaseOrders
{
    public record PurchaseOrderDto(
          int Id,
          string PurchaseOrderCode,
          DateTime Date,
          int SupplierId,
          bool HasVAT,
          List<PurchaseOrderItemDto> Items
      );
}
