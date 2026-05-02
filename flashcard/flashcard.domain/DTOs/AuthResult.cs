using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace flashcard.domain.DTOs;

public class AuthResult
{
    public required bool Successful { get; set; }
    public required string ErrorMessage { get; set; }
    public TokenInfo? TokenInfo { get; set; }
}
