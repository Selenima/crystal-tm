using Crystal.Web.Models;
using Microsoft.AspNetCore.Identity;

namespace Crystal.Web.Data;


// маленький вспомогательный класс чтобы получить хеши паролей 
public static class SeederPassword
{
    public static string Hash(string password)
    {
        var hasher = new PasswordHasher<User>();
        return hasher.HashPassword(new User(), password);
    }
}
