using flashcard.domain.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace flashcard.application.ServiceContracts;

public interface ITokenGenerator
{
    public AccessTokenInfo GenerateAccessToken(string username, string userId, IList<string>? roles);
    public string GenerateRefreshToken();
}
