using Domain.Common;
using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Repositories
{
    public class PurchaseOrderRepository : IPurchaseOrderRepository
    {
        private readonly Context _context;
        private readonly ILogger<PurchaseOrderRepository> _logger;

        public PurchaseOrderRepository(Context context, ILogger<PurchaseOrderRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<PagedList<PurchaseOrder>> GetFilteredOrdersAsync(
            DateTime? dateFrom,
            DateTime? dateTo,
            int? supplierId,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Getting filtered orders - DateFrom: {DateFrom}, DateTo: {DateTo}, SupplierId: {SupplierId}, Page: {PageNumber}, Size: {PageSize}",
                dateFrom, dateTo, supplierId, pageNumber, pageSize);

            var query = _context.Set<PurchaseOrder>()
                .AsNoTracking()
                .AsQueryable();

            // 2. Apply Filters
            if (dateFrom.HasValue)
            {
                query = query.Where(x => x.Date >= dateFrom.Value);
            }

            if (dateTo.HasValue)
            {
                query = query.Where(x => x.Date <= dateTo.Value);
            }

            if (supplierId.HasValue)
            {
                query = query.Where(x => x.SupplierId == supplierId.Value);
            }

            // 3. Ordering (Usually by Date Descending)
            query = query.OrderByDescending(x => x.Date).ThenByDescending(x => x.Id);

            // 4. Pagination Calculation
            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            _logger.LogInformation("Retrieved {Count} orders out of {TotalCount}", items.Count, totalCount);

            // 5. Return Paged Result
            return PagedList<PurchaseOrder>.Create(items, totalCount, pageNumber, pageSize);
        }

        public async Task<PurchaseOrder?> GetByIdAsync(int id, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Getting purchase order by Id: {Id}", id);

            var order = await _context.Set<PurchaseOrder>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (order == null)
            {
                _logger.LogWarning("Purchase order with Id {Id} not found", id);
            }

            return order;
        }

        public async Task<PurchaseOrder?> GetByIdWithItemsAsync(int id, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Getting purchase order with items by Id: {Id}", id);

            return await _context.Set<PurchaseOrder>()
             .Include(order => order.Items)
             .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        }

        public async Task AddAsync(PurchaseOrder order, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Adding new purchase order for Supplier: {SupplierId}", order.SupplierId);

            await _context.Set<PurchaseOrder>().AddAsync(order, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Purchase order {Id} added successfully", order.Id);
        }

        public async Task UpdateAsync(PurchaseOrder order, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Updating purchase order {Id}", order.Id);

            _context.Set<PurchaseOrder>().Update(order);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Purchase order {Id} updated successfully", order.Id);
        }

        public async Task DeleteAsync(PurchaseOrder order, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Deleting purchase order {Id}", order.Id);

            _context.Set<PurchaseOrder>().Remove(order);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Purchase order {Id} deleted successfully", order.Id);
        }

        public async Task<bool> ExistsAsync(int id, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Checking if purchase order exists: {Id}", id);

            var exists = await _context.Set<PurchaseOrder>()
                .AnyAsync(x => x.Id == id, cancellationToken);

            _logger.LogInformation("Purchase order {Id} exists: {Exists}", id, exists);

            return exists;
        }

        public async Task<PagedList<PurchaseOrder>> GetOrdersAsync(PaginationParameters pagination,CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                "Getting orders - Page: {PageNumber}, Size: {PageSize}",
                pagination.PageNumber,
                pagination.PageSize);

            var query = _context.Set<PurchaseOrder>()
                .AsNoTracking()
                .OrderByDescending(x => x.Date)
                .ThenByDescending(x => x.Id);

            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query
                .Skip((pagination.PageNumber - 1) * pagination.PageSize)
                .Take(pagination.PageSize)
                .ToListAsync(cancellationToken);

            _logger.LogInformation(
                "Retrieved {Count} orders out of {TotalCount}",
                items.Count,
                totalCount);

            return PagedList<PurchaseOrder>.Create(
                items,
                totalCount,
                pagination.PageNumber,
                pagination.PageSize);
        }

    }
}
