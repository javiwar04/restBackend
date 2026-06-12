using System.Security.Cryptography;
using System.Text;

namespace WebApi.Services;

public class HashService
{
    public string HashPin(string pin)
    {
        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(pin));
        return Convert.ToHexString(bytes).ToLower();
    }

    public bool VerifyPin(string pin, string hash)
    {
        var computedHash = HashPin(pin);
        return computedHash.Equals(hash, StringComparison.OrdinalIgnoreCase);
    }
}
