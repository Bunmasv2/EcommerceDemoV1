public class PasswordService : IPasswordService
{
    public Task<string> HashPasswordAsync(string password)
    {
        return Task.FromResult(BCrypt.Net.BCrypt.HashPassword(password));
    }

    public Task<bool> VerifyPasswordAsync(string hashedPassword, string providedPassword)
    {
        return Task.FromResult(BCrypt.Net.BCrypt.Verify(providedPassword, hashedPassword));
    }
}