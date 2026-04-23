public interface IPasswordService
{
    Task<string> HashPasswordAsync(string password);
    Task<bool> VerifyPasswordAsync(string hashedPassword, string providedPassword);
}