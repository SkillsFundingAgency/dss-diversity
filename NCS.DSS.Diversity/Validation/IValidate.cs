using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using NCS.DSS.Diversity.Models;

namespace NCS.DSS.Diversity.Validation
{
    public interface IValidate
    {
        List<ValidationResult> ValidateResource(IDiversity diversity);
    }
}