namespace Dobby.DTOs;

public record RegisterRequestDto(
    string Username,
    string Password
);

public record LoginRequestDto(
    string Username,
    string Password
);

public record AuthResponseDto(
    string Message,
    Guid Id,
    string Username,
    string Token
);
