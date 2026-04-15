using EcommerceDemoV1.Application.DTOs.Auth;
using MediatR;

public record LoginUserCommand(string Email, string Password) : IRequest<AuthResponse>;