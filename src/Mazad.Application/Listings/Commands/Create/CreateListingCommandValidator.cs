using FluentValidation;
using Mazad.Domain.Enums;

namespace Mazad.Application.Listings.Commands.Create;

public class CreateListingCommandValidator : AbstractValidator<CreateListingCommand>
{
    public CreateListingCommandValidator()
    {
        RuleFor(x => x.SellerId).NotEmpty();
        RuleFor(x => x.CategoryId).NotEmpty();
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).NotEmpty();
        RuleFor(x => x.Type).IsInEnum();

        RuleFor(x => x.StartPrice).GreaterThanOrEqualTo(0).When(x => x.StartPrice.HasValue);
        RuleFor(x => x.ReservePrice).GreaterThanOrEqualTo(0).When(x => x.ReservePrice.HasValue);
        RuleFor(x => x.BidIncrement).GreaterThan(0).When(x => x.BidIncrement.HasValue);
        RuleFor(x => x.BuyNowPrice).GreaterThan(0).When(x => x.Type != ListingType.Auction);

        When(x => x.StartAt.HasValue && x.EndAt.HasValue, () =>
        {
            RuleFor(x => x.EndAt).GreaterThan(x => x.StartAt);
        });

        RuleForEach(x => x.Media).ChildRules(media =>
        {
            media.RuleFor(m => m.Url).NotEmpty().MaximumLength(2048);
            media.RuleFor(m => m.Type).NotEmpty();
            media.RuleFor(m => m.SortOrder).GreaterThanOrEqualTo(0).When(m => m.SortOrder.HasValue);
        });
    }
}
