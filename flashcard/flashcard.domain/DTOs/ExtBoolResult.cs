using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace flashcard.domain.DTOs;

public class ExtBoolResult
{
    public required bool Successful { get; set; }
    public required string ErrorMessage { get; set; }
}
