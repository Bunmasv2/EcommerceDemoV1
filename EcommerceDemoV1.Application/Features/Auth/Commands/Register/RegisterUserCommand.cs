using MediatR;

public record RegisterUserCommand(string FullName, string Email, string Password, string Role) : IRequest<int>;