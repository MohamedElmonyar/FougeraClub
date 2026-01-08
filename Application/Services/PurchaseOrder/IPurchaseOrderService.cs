using Application.DTOs.PurchaseOrders;
using Domain.Common;


namespace Application.Services.PurchaseOrder
{
    public interface IPurchaseOrderService
    {
        Task<PagedList<PurchaseOrderDto>> GetFilteredOrdersAsync(DateTime? dateFrom, DateTime? dateTo, int? supplierId, int pageNumber, int pageSize, CancellationToken cancellationToken);

        Task<PagedList<PurchaseOrderDto>> GetOrdersAsync(PaginationParameters pagination, CancellationToken cancellationToken);

        Task<PurchaseOrderDto> GetByIdAsync(int id, CancellationToken cancellationToken);

        Task<PurchaseOrderDto> GetByIdWithItemsAsync(int id, CancellationToken cancellationToken);

        Task<int> CreateAsync(PurchaseOrderDto order, CancellationToken cancellationToken);

        Task UpdateAsync(PurchaseOrderDto order, CancellationToken cancellationToken);

        Task DeleteAsync(int id, CancellationToken cancellationToken);
    }
}
