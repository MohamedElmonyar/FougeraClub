using Application.DTOs.PurchaseOrders;
using Application.Services.PurchaseOrder;
using AutoMapper;
using Domain.Common;
using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using Microsoft.Extensions.Logging;


public class PurchaseOrderService : IPurchaseOrderService
{
    private readonly IPurchaseOrderRepository _repository;
    private readonly ILogger<PurchaseOrderService> _logger;
    private readonly IMapper _mapper;

    public PurchaseOrderService(
        IPurchaseOrderRepository repository,
        ILogger<PurchaseOrderService> logger,
        IMapper mapper)
    {
        _repository = repository;
        _logger = logger;
        _mapper = mapper;
    }

    public async Task<PagedList<PurchaseOrderDto>> GetFilteredOrdersAsync(
        DateTime? dateFrom,
        DateTime? dateTo,
        int? supplierId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Getting filtered PurchaseOrders. DateFrom: {DateFrom}, DateTo: {DateTo}, SupplierId: {SupplierId}, Page: {PageNumber}, Size: {PageSize}",
            dateFrom, dateTo, supplierId, pageNumber, pageSize);

        // Get Entities from Repo
        var result = await _repository.GetFilteredOrdersAsync(
            dateFrom, dateTo, supplierId, pageNumber, pageSize, cancellationToken);

        // Map Entities to DTOs
        var dtos = _mapper.Map<List<PurchaseOrderDto>>(result.Items);

        _logger.LogInformation("Filtered PurchaseOrders retrieved successfully");

        // Return Mapped PagedList
        return new PagedList<PurchaseOrderDto>(dtos, result.TotalCount, result.CurrentPage, result.PageSize);
    }

    public async Task<PagedList<PurchaseOrderDto>> GetOrdersAsync(
        PaginationParameters pagination,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Getting PurchaseOrders. Page: {PageNumber}, Size: {PageSize}",
            pagination.PageNumber, pagination.PageSize);

        // Get Entities from Repo
        var result = await _repository.GetOrdersAsync(pagination, cancellationToken);

        // Map Entities to DTOs
        var dtos = _mapper.Map<List<PurchaseOrderDto>>(result.Items);

        _logger.LogInformation("PurchaseOrders retrieved successfully");

        // Return Mapped PagedList
        return new PagedList<PurchaseOrderDto>(dtos, result.TotalCount, result.CurrentPage, result.PageSize);
    }

    public async Task<PurchaseOrderDto> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting PurchaseOrder by Id {PurchaseOrderId}", id);

        if (id <= 0)
        {
            _logger.LogWarning("Invalid PurchaseOrderId received: {PurchaseOrderId}", id);
            throw new ArgumentException("Invalid purchase order id.");
        }

        var order = await _repository.GetByIdAsync(id, cancellationToken);

        if (order is null)
        {
            _logger.LogWarning("PurchaseOrder not found. Id: {PurchaseOrderId}", id);
            throw new KeyNotFoundException("Purchase order not found.");
        }

        _logger.LogInformation("PurchaseOrder retrieved successfully. Id: {PurchaseOrderId}", id);

        return _mapper.Map<PurchaseOrderDto>(order);
    }

    public async Task<PurchaseOrderDto> GetByIdWithItemsAsync(int id, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting PurchaseOrder with items by Id {PurchaseOrderId}", id);

        if (id <= 0)
        {
            _logger.LogWarning("Invalid PurchaseOrderId received: {PurchaseOrderId}", id);
            throw new ArgumentException("Invalid purchase order id.");
        }

        var order = await _repository.GetByIdWithItemsAsync(id, cancellationToken);

        if (order is null)
        {
            _logger.LogWarning("PurchaseOrder not found. Id: {PurchaseOrderId}", id);
            throw new KeyNotFoundException("Purchase order not found.");
        }

        _logger.LogInformation("PurchaseOrder with items retrieved successfully. Id: {PurchaseOrderId}", id);

        // Map to DTO
        return _mapper.Map<PurchaseOrderDto>(order);
    }

    public async Task<int> CreateAsync(PurchaseOrderDto dto, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Creating PurchaseOrder for Supplier {SupplierId}",
            dto.SupplierId);

        var entity = _mapper.Map<PurchaseOrder>(dto);

        // Ensure Status is set correctly for new items
        entity.Status = PurchaseOrderStatus.Draft;

        await _repository.AddAsync(entity, cancellationToken);

        _logger.LogInformation(
            "PurchaseOrder created successfully with Id {PurchaseOrderId}",
            entity.Id);

        return entity.Id;
    }

    public async Task UpdateAsync(PurchaseOrderDto dto, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating PurchaseOrder Id {PurchaseOrderId}", dto.Id);

        if (dto.Id <= 0)
        {
            _logger.LogWarning("Invalid PurchaseOrderId for update: {PurchaseOrderId}", dto.Id);
            throw new ArgumentException("Invalid purchase order id.");
        }

        // We need to fetch the existing entity first (with items to handle updates correctly)
        var existingEntity = await _repository.GetByIdWithItemsAsync(dto.Id, cancellationToken);

        if (existingEntity == null)
        {
            _logger.LogWarning("Attempt to update non-existing PurchaseOrder Id {PurchaseOrderId}", dto.Id);
            throw new KeyNotFoundException("Purchase order not found.");
        }

        // Map changes from DTO to existing Entity
        _mapper.Map(dto, existingEntity);

        await _repository.UpdateAsync(existingEntity, cancellationToken);

        _logger.LogInformation("PurchaseOrder updated successfully. Id {PurchaseOrderId}", dto.Id);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting PurchaseOrder Id {PurchaseOrderId}", id);

        if (id <= 0)
        {
            _logger.LogWarning("Invalid PurchaseOrderId for delete: {PurchaseOrderId}", id);
            throw new ArgumentException("Invalid purchase order id.");
        }

        var order = await _repository.GetByIdAsync(id, cancellationToken);

        if (order is null)
        {
            _logger.LogWarning("Attempt to delete non-existing PurchaseOrder Id {PurchaseOrderId}", id);
            throw new KeyNotFoundException("Purchase order not found.");
        }

        await _repository.DeleteAsync(order, cancellationToken);

        _logger.LogInformation("PurchaseOrder deleted successfully. Id {PurchaseOrderId}", id);
    }
}