using MediatR;

public record RegisterUserCommand(string Email, string Password, string Role) : IRequest<int>;