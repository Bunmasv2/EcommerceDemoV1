using EcommerceDemoV1.Application.DTOs.Auth;
using MediatR;

public class LoginUserHandler : IRequestHandler<LoginUserCommand, AuthResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtRepository _jwtRepository;

    public LoginUserHandler(IUserRepository userRepository, IJwtRepository jwtRepository)
    {
        _userRepository = userRepository;
        _jwtRepository = jwtRepository;
    }
    public async Task<AuthResponse> Handle(LoginUserCommand loginUserCommand, CancellationToken cancellationToken)
    {

        var user = await _userRepository.GetByEmailAsync(loginUserCommand.Email);
        if (user == null)
            throw new Exception("Invalid email or password");

        var validPassword = BCrypt.Net.BCrypt.Verify(loginUserCommand.Password, user.PasswordHash);
        if (!validPassword)
            throw new Exception("Invalid email or password");
        return new AuthResponse
        {
            AccessToken = _jwtRepository.GenerateToken(user.Id, user.Email, user.Role),
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