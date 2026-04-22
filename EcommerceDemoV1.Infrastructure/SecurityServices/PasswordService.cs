public class PasswordService : IPasswordService
{
    public Task<string> HashPasswordAsync(string password)
    => Task.Run(() => BCrypt.Net.BCrypt.HashPassword(password));

    public Task<bool> VerifyPasswordAsync(string hashedPassword, string providedPassword)
        => Task.Run(() => BCrypt.Net.BCrypt.Verify(providedPassword, hashedPassword));
}