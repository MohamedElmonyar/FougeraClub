using Domain.Common;
using Domain.Entities;


namespace Domain.Interfaces
{
    public interface IPurchaseOrderRepository
    {
        Task<PagedList<PurchaseOrder>> GetFilteredOrdersAsync(DateTime? dateFrom, DateTime? dateTo,int? supplierId,int pageNumber,int pageSize,CancellationToken cancellationToken);
        Task AddAsync(PurchaseOrder order, CancellationToken cancellationToken);
        Task<PurchaseOrder?> GetByIdWithItemsAsync(int id, CancellationToken cancellationToken);
        Task<PurchaseOrder?> GetByIdAsync(int id, CancellationToken cancellationToken);
        Task UpdateAsync(PurchaseOrder order, CancellationToken cancellationToken);
        Task DeleteAsync(PurchaseOrder order, CancellationToken cancellationToken);
        Task<bool> ExistsAsync(int id, CancellationToken cancellationToken);
        Task<PagedList<PurchaseOrder>> GetOrdersAsync(PaginationParameters pagination, CancellationToken cancellationToken);

    }
}
