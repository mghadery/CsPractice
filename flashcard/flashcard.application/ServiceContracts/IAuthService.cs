using flashcard.domain.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace flashcard.application.ServiceContracts;

public interface IAuthService
{
    public Task<AuthResult> Login(UserLoginDTO loginDTO);
    public Task<AuthResult> RefreshToken(TokenRefreshDTO token);

    public Task<ExtBoolResult> Logout(TokenRefreshDTO token);
    public Task<ExtBoolResult> Register(UserRegisterDTO registerDTO);
}
