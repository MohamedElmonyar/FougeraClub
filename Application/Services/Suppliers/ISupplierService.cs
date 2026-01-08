using Microsoft.AspNetCore.Mvc.Rendering;


namespace Application.Services.Suppliers
{
    public interface ISupplierService
    {
        List<SelectListItem> GetSuppliersDropdown();
    }
}
