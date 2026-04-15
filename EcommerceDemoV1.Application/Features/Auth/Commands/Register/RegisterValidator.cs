using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;
namespace EcommerceDemoV1.Application.Features.Auth.Commands.Register
{
    public class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
    {
        public RegisterUserCommandValidator(IUserRepository userRepo)
        {
            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("Tên người dùng không Được để trống")
                .MinimumLength(3).WithMessage("Tên người dùng có ít nhất là 3 kí tự");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email cũng không được để trống lun nhé")
                .EmailAddress(FluentValidation.Validators.EmailValidationMode.AspNetCoreCompatible).WithMessage("Định dạng không hợp lệ")
                .MustAsync(async (email, cancellation) => !await userRepo.ExistsByEmailAsync(email))
                .WithMessage("Email này đã tồn tại");
            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Không được bỏ trống password!")
                .MinimumLength(6).WithMessage("Password phải có ít nhất 6 kí tự");
        }
    }
}