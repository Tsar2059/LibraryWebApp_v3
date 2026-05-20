using System.Security.Cryptography;
using System.Text;

namespace LibraryWebApp.Services;

public class AuthService
{
    // Хеш пароля "admin123" (можно изменить на любой)
    // Для нового пароля: HashPassword("ваш_пароль")
    private const string AdminPasswordHash = "0192023a7bbd73250516f069df18b500"; // admin123
    
    private const string AdminUserName = "admin";
    private bool _isAuthenticated = false;

    public bool IsAuthenticated => _isAuthenticated;

    public bool Login(string username, string password)
    {
        if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
        {
            if (username == AdminUserName && HashPassword(password) == AdminPasswordHash)
            {
                _isAuthenticated = true;
                return true;
            }
        }
        return false;
    }

    public void Logout()
    {
        _isAuthenticated = false;
    }

    private static string HashPassword(string password)
    {
        using var md5 = MD5.Create();
        var bytes = md5.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToHexString(bytes).ToLower();
    }
}