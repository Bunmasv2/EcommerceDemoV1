using MediatR;

public record RegisterUserCommand(string FullName, string Email, string Password) : IRequest<int>;