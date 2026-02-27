using System.Security.Cryptography;
using System.Text;

namespace HizliOgren.Services;

public class AuthService
{
    public string HashPassword(string password)
    {
        var salt = new byte[16];
        RandomNumberGenerator.Fill(salt);
        var hash = Rfc2898DeriveBytes.Pbkdf2(Encoding.UTF8.GetBytes(password), salt, 100000, HashAlgorithmName.SHA256, 32);
        var combined = new byte[salt.Length + hash.Length];
        Buffer.BlockCopy(salt, 0, combined, 0, salt.Length);
        Buffer.BlockCopy(hash, 0, combined, salt.Length, hash.Length);
        return Convert.ToBase64String(combined);
    }

    public bool VerifyPassword(string password, string storedHash)
    {
        if (string.IsNullOrWhiteSpace(storedHash)) return false;
        try
        {
            var combined = Convert.FromBase64String(storedHash.Trim());
            if (combined.Length < 48) return false; // 16 salt + 32 hash
            var salt = new byte[16];
            Buffer.BlockCopy(combined, 0, salt, 0, 16);
            var hash = Rfc2898DeriveBytes.Pbkdf2(Encoding.UTF8.GetBytes(password ?? ""), salt, 100000, HashAlgorithmName.SHA256, 32);
            for (int i = 0; i < 32; i++)
                if (combined[16 + i] != hash[i]) return false;
            return true;
        }
        catch
        {
            return false;
        }
    }
}
