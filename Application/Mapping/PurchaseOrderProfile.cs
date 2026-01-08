using Application.DTOs.PurchaseOrders;
using AutoMapper;
using Domain.Entities;

namespace Application.Mapping
{
    public class PurchaseOrderProfile : Profile
    {
        public PurchaseOrderProfile()
        {
            // From Form (DTO) to Database (Entity)
            CreateMap<PurchaseOrderDto, PurchaseOrder>()
                  .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items));

            CreateMap<PurchaseOrderItemDto, PurchaseOrderItem>();

            // From Entity to DTO (Get/Read)
            CreateMap<PurchaseOrder, PurchaseOrderDto>();
            CreateMap<PurchaseOrderItem, PurchaseOrderItemDto>();

            // Note: SavePurchaseOrderDto maps are kept if needed elsewhere, 
            // but we focused on the new PurchaseOrderDto as requested.
            CreateMap<PurchaseOrderDto, PurchaseOrder>();
            CreateMap<PurchaseOrder, PurchaseOrderDto>();
        }
    }
}
