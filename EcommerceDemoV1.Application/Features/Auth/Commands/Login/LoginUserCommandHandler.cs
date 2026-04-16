using EcommerceDemoV1.Application.DTOs.Auth;
using MediatR;

public class LoginUserHandler : IRequestHandler<LoginUserCommand, AuthResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtService _jwtService;
    private readonly IPasswordService _passwordService;

    public LoginUserHandler(IUserRepository userRepository, IJwtService jwtService, IPasswordService passwordService)
    {
        _userRepository = userRepository;
        _jwtService = jwtService;
        _passwordService = passwordService;
    }
    public async Task<AuthResponse> Handle(LoginUserCommand loginUserCommand, CancellationToken cancellationToken)
    {

        var user = await _userRepository.GetByEmailAsync(loginUserCommand.Email);
        if (user == null)
            throw new Exception("Invalid email or password");

        var validPassword = await _passwordService.VerifyPasswordAsync(user.PasswordHash, loginUserCommand.Password);
        if (!validPassword)
            throw new Exception("Invalid email or password");
        return new AuthResponse
        {
            AccessToken = _jwtService.GenerateToken(user.Id, user.Email, user.Role),
            infoUser = new
            {
                user.Id,
                user.Email,
                user.Role,
                user.MemberRank
            }
        };
    }

}