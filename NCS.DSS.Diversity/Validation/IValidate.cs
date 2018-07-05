using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NCS.DSS.Diversity.Validation
{
    public interface IValidate
    {
        List<ValidationResult> ValidateResource(Models.Diversity diversity);
    }
}