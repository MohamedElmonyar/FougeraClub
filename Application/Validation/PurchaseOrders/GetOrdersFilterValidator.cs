using Application.DTOs.PurchaseOrders;
using FluentValidation;


namespace Application.Validation.PurchaseOrders
{
    public class GetOrdersFilterValidator : AbstractValidator<GetOrdersFilterDto>
    {
        public GetOrdersFilterValidator()
        {
            RuleFor(x => x.PageNumber)
                .GreaterThan(0).WithMessage("Page Number must be greater than 0.");

            RuleFor(x => x.PageSize)
                .GreaterThan(0).WithMessage("Page Size must be greater than 0.")
                .LessThanOrEqualTo(100).WithMessage("Page Size cannot exceed 100 records.");

            RuleFor(x => x.DateTo)
                .GreaterThanOrEqualTo(x => x.DateFrom)
                .When(x => x.DateFrom.HasValue && x.DateTo.HasValue)
                .WithMessage("End Date must be greater than or equal to Start Date.");
        }
    }
}
