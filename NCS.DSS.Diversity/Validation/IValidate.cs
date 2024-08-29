using NCS.DSS.Diversity.Models;
using System.ComponentModel.DataAnnotations;

namespace NCS.DSS.Diversity.Validation
{
    public interface IValidate
    {
        List<ValidationResult> ValidateResource(IDiversity diversity);
    }
}