using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace flashcard.domain.DTOs;

public class TokenRefreshDTO
{
    [Required]
    public string RefreshToken { get; set; } = string.Empty;
}
