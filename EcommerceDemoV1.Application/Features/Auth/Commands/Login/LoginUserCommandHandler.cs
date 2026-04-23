using EcommerceDemoV1.Application.DTOs.Auth;
using MediatR;

public class LoginUserHandler : IRequestHandler<LoginUserCommand, AuthResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtService _jwtService;
    private readonly IPasswordService _passwordService;
    private readonly IUnitOfWork _unitOfWork;

    public LoginUserHandler(IUserRepository userRepository, IJwtService jwtService, IPasswordService passwordService, IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _jwtService = jwtService;
        _passwordService = passwordService;
        _unitOfWork = unitOfWork;
    }
    public async Task<AuthResponse> Handle(LoginUserCommand loginUserCommand, CancellationToken cancellationToken)
    {

        var user = await _userRepository.GetByEmailAsync(loginUserCommand.Email);
        if (user == null)
            throw new Exception("Invalid email or password");

        var validPassword = await _passwordService.VerifyPasswordAsync(user.PasswordHash, loginUserCommand.Password);
        if (!validPassword)
            throw new Exception("Invalid email or password");

        var refreshToken = _jwtService.GenerateRefreshToken();
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
        await _userRepository.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new AuthResponse
        {
            AccessToken = _jwtService.GenerateToken(user.Id, user.Email, user.Role),
            RefreshToken = refreshToken,
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