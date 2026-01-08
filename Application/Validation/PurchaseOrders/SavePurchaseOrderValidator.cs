using Application.DTOs.PurchaseOrders;
using FluentValidation;


namespace Application.Validation.PurchaseOrders
{
    public class SavePurchaseOrderValidator : AbstractValidator<PurchaseOrderDto>
    {
        public SavePurchaseOrderValidator()
        {
            RuleFor(x => x.PurchaseOrderCode)
                .NotEmpty().WithMessage("Purchase Order Code is required.")
                .MaximumLength(50).WithMessage("Code cannot exceed 50 characters.");

            RuleFor(x => x.Date)
                .NotEmpty().WithMessage("Date is required.")
                .LessThanOrEqualTo(DateTime.Now.AddDays(1)).WithMessage("Date cannot be in the far future.");

            RuleFor(x => x.SupplierId)
                .GreaterThan(0).WithMessage("Please select a valid supplier.");

            RuleFor(x => x.Items)
                .NotEmpty().WithMessage("You must add at least one item to the order.");

            // Validate the list items
            RuleForEach(x => x.Items).SetValidator(new PurchaseOrderItemValidator());
        }
    }
}
