using FluentValidation;
using Mazad.Application.Auth.Commands;

namespace Mazad.Application.Auth.Validators;

public class EnableAuthenticatorMfaCommandValidator : AbstractValidator<EnableAuthenticatorMfaCommand>
{
    public EnableAuthenticatorMfaCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty();

        RuleFor(x => x.Code)
            .MaximumLength(10)
            .When(x => x.Code is not null);
    }
}
