using Application.DTOs.PurchaseOrders;
using FluentValidation;


namespace Application.Validation.PurchaseOrders
{
    public class PurchaseOrderItemValidator : AbstractValidator<PurchaseOrderItemDto>
    {
        public PurchaseOrderItemValidator()
        {
            RuleFor(x => x.ItemName)
                .NotEmpty().WithMessage("Item Name is required.");

            RuleFor(x => x.Quantity)
                .GreaterThan(0).WithMessage("Quantity must be greater than 0.");

            RuleFor(x => x.SinglePrice)
                .GreaterThan(0).WithMessage("Price must be greater than 0.");
        }
    }
}
