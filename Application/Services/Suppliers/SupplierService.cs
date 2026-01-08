using Microsoft.AspNetCore.Mvc.Rendering;


namespace Application.Services.Suppliers
{
    public class SupplierService : ISupplierService
    {
        public List<SelectListItem> GetSuppliersDropdown()
        {
            return new List<SelectListItem>
            {
              new SelectListItem { Value = "1", Text = "Al Fujeirah Bookstore" },
              new SelectListItem { Value = "2", Text = "Etisalat Telecommunications" },
              new SelectListItem { Value = "3", Text = "ADNOC Distribution" },
            };
        }
    }
}
