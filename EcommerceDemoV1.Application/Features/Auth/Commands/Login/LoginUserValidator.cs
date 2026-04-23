using FluentValidation;

namespace Application.Features.Auth.Commands.Login
{
    public class LoginCommandValidator : AbstractValidator<LoginUserCommand>
    {

        public LoginCommandValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email không được để trống")
                .When(x => x.Email != null);
            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Thiếu mật khẩu");

        }
    }
}