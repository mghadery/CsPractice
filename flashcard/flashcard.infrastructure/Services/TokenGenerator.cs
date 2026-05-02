using flashcard.application.ServiceContracts;
using flashcard.domain.DTOs;
using flashcard.infrastructure.Settings;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace flashcard.infrastructure.Services;

public class TokenGenerator(IOptions<JwtSettings> options) : ITokenGenerator
{
    public AccessTokenInfo GenerateAccessToken(string username, string userId, IList<string>? roles)
    {
        var jwtSettings = options.Value;

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, username ),
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(JwtRegisteredClaimNames.Iat, ((DateTimeOffset)DateTime.UtcNow).ToUnixTimeSeconds().ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Sub, userId)
        };
        if (roles is not null && roles.Any())
            claims.AddRange(roles.Select(x => new Claim(ClaimTypes.Role, x)));

        var accessExpiration = DateTime.UtcNow.AddMinutes(jwtSettings.AccessExpiryMinutes);
        
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key));
        var signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        var jwtSecurityToken = new JwtSecurityToken(jwtSettings.Issuer, jwtSettings.Audience, claims, null, accessExpiration, signingCredentials);
        var accessToken = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);

        return new AccessTokenInfo()
        {
            AccessToken = accessToken,
            AccessExpiration = accessExpiration
        };
    }

    public string GenerateRefreshToken()
    {
        using (var rng = RandomNumberGenerator.Create())
        {
            byte[] bytes = new byte[64];
            rng.GetBytes(bytes);
            return Convert.ToBase64String(bytes);
        }
    }
}
