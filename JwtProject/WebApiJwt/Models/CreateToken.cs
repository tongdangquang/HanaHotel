using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace WebApiJwt.Models
{
    public class CreateToken
    {
        public string Create()
        {
            // Gizli anahtarı byte dizisine çeviriyoruz
            var bytes = Encoding.UTF8.GetBytes("aspnetcoreapiapisecretkeyforjwt12345");

            // Simetrik güvenlik anahtarı oluşturuyoruz
            SymmetricSecurityKey key = new SymmetricSecurityKey(bytes);

            // İmza bilgilerini (signing credentials) oluşturuyoruz
            SigningCredentials credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // JWT token'ını oluşturuyoruz
            JwtSecurityToken jwtSecurityToken = new JwtSecurityToken(
                issuer: "http://localhost",          // Token'ı oluşturan (issuer)
                audience: "https://localhost",        // Token'ı kullanacak olanlar (audience)
                notBefore: DateTime.Now,              // Token'ın kullanılmaya başlanabileceği zaman
                expires: DateTime.Now.AddSeconds(30),  // Token'ın geçerlilik süresi
                signingCredentials: credentials       // İmza bilgileri
            );

            // Token'ı string formatına çevirip döndürüyoruz
            return new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
        }

        public string CreateAdmin()
        {
            var bytes = Encoding.UTF8.GetBytes("aspnetcoreapiapisecretkeyforjwt12345");
            SymmetricSecurityKey key = new SymmetricSecurityKey(bytes);
            SigningCredentials credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            List<Claim> claims = new List<Claim>()
            {
                new (ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
                new (ClaimTypes.Role, "Admin"),
                new (ClaimTypes.Role, "Guest")
            };

            JwtSecurityToken jwtSecurityToken = new JwtSecurityToken(
                issuer: "http://localhost",          // Token'ı oluşturan (issuer)
                audience: "https://localhost",        // Token'ı kullanacak olanlar (audience)
                claims: claims,                      // Token içindeki talepler
                notBefore: DateTime.Now,              // Token'ın kullanılmaya başlanabileceği zaman
                expires: DateTime.Now.AddSeconds(30),  // Token'ın geçerlilik süresi
                signingCredentials: credentials       // İmza bilgileri
            );

            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
            return tokenHandler.WriteToken(jwtSecurityToken);
        }
    }

}