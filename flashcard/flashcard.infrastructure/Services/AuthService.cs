using flashcard.application.ServiceContracts;
using flashcard.domain.DTOs;
using flashcard.infrastructure.DbContext;
using flashcard.infrastructure.IdentityEntities;
using flashcard.infrastructure.Settings;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace flashcard.infrastructure.Services;

public class AuthService(ITokenGenerator tokenGenerator, IOptions<JwtSettings> options, AppDbContext dbContext, UserManager<User> userManager) : IAuthService
{
    public async Task<AuthResult> Login(UserLoginDTO loginDTO)
    {
        //check user existance
        var user = await userManager.FindByNameAsync(loginDTO.Username);
        if (user is null)
        {
            return new AuthResult()
            {
                Successful = false,
                ErrorMessage = "Invalid username"
            };
        }
        if (!await userManager.CheckPasswordAsync(user, loginDTO.Password))
        {
            return new AuthResult()
            {
                Successful = false,
                ErrorMessage = "Invalid password"
            };
        }

        var roles = await userManager.GetRolesAsync(user);

        var jwtSettings = options.Value;
        var accessTokenInfo = tokenGenerator.GenerateAccessToken(loginDTO.Username, user.Id.ToString(), roles);

        string refreshToken;
        while (true)
        {
            refreshToken = tokenGenerator.GenerateRefreshToken();
            //available = !await dbContext.RefreshTokens.AnyAsync(x => x.Token == refreshToken);
            var refreshExpiration = DateTime.UtcNow.AddMinutes(jwtSettings.RefreshExpiryMinutes);

            //insert refreshtoken
            try
            {
                await dbContext.RefreshTokens.AddAsync(new RefreshToken()
                {
                    Token = refreshToken,
                    ExpirationDate = refreshExpiration,
                    UserId = user.Id,
                    Id = Guid.NewGuid()
                });
                await dbContext.SaveChangesAsync();
                return new AuthResult()

                {
                    Successful = true,
                    ErrorMessage = string.Empty,

                    TokenInfo = new TokenInfo()
                    {
                        AccessToken = accessTokenInfo.AccessToken,
                        AccessExpiration = accessTokenInfo.AccessExpiration,
                        RefreshToken = refreshToken,
                        RefreshExpiration = refreshExpiration
                    }
                };
            }
            catch (DbUpdateException)
            {

            }
            catch { throw; }
        };
    }

    public Task<ExtBoolResult> Logout(TokenRefreshDTO token)
    {
        throw new NotImplementedException();
    }

    public Task<AuthResult> RefreshToken(TokenRefreshDTO token)
    {
        throw new NotImplementedException();
    }

    public async Task<ExtBoolResult> Register(UserRegisterDTO registerDTO)
    {
        //check if username is available
        var user = await userManager.FindByNameAsync(registerDTO.Username);
        if (user is not null)
            return new ExtBoolResult()
            {
                ErrorMessage = "Username exists",
                Successful = false
            };

        //check if its is the first user

        //find user role

        user = new User()
        {
            UserName = registerDTO.Username,
            Email = registerDTO.Email,
            EmailConfirmed = true
        };
        var idRes = await userManager.CreateAsync(user, registerDTO.Password);
        if (idRes.Succeeded)
            return new ExtBoolResult()
            {
                ErrorMessage = string.Empty,
                Successful = true
            };

        return new ExtBoolResult()
        {
            ErrorMessage = string.Join("-", idRes.Errors.Select(x => x.Description)),
            Successful = false
        };
    }
}
