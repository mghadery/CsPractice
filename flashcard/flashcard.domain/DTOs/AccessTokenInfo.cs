using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace flashcard.domain.DTOs;

public class AccessTokenInfo
{
    public required string AccessToken { get; set; }
    public required DateTime AccessExpiration { get; set; }
}
