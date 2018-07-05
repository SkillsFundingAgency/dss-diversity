using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NCS.DSS.Diversity.Validation
{
    public class Validate : IValidate
    {
        public List<ValidationResult> ValidateResource(Models.Diversity diversity)
        {
            var context = new ValidationContext(diversity, null, null);
            var results = new List<ValidationResult>();

            var isValid = Validator.TryValidateObject(diversity, context, results, true);

            return isValid ? null : results;
        }
    }
}
