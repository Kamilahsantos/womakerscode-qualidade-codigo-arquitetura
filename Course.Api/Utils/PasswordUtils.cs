using System.Security.Cryptography;
using System.Text;

namespace Course.Api.Utils;

public static class PasswordUtils
{
    // In production, prefer BCrypt or Argon2 over SHA-256
    public static string HashSenha(string senha)
    {
        using var sha256 = SHA256.Create();
        var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(senha));
        return Convert.ToHexString(hash);
    }
}
