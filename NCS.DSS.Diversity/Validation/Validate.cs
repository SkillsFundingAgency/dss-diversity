using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using NCS.DSS.Diversity.Models;
using NCS.DSS.Diversity.ReferenceData;

namespace NCS.DSS.Diversity.Validation
{
    public class Validate : IValidate
    {
        public List<ValidationResult> ValidateResource(IDiversity diversity)
        {
            var context = new ValidationContext(diversity, null, null);
            var results = new List<ValidationResult>();

            Validator.TryValidateObject(diversity, context, results, true);
            ValidateDiversityRules(diversity, results);

            return results;
        }

        private void ValidateDiversityRules(IDiversity diversityResource, List<ValidationResult> results)
        {
            if (diversityResource == null)
                return;

            if(diversityResource.ConsentToCollectLLDDHealth.GetValueOrDefault() && !diversityResource.DateAndTimeLLDDHealthConsentCollected.HasValue)
                results.Add(new ValidationResult("Date And Time LLDD Health Consent Collected must have a value", new[] { "DateAndTimeLLDDHealthConsentCollected" }));

            if (diversityResource.DateAndTimeLLDDHealthConsentCollected.HasValue && diversityResource.DateAndTimeLLDDHealthConsentCollected.Value > DateTime.UtcNow)
                results.Add(new ValidationResult("Date And Time LLDD Health Consent Collected must be less the current date/time", new[] { "DateAndTimeLLDDHealthConsentCollected" }));

            if (diversityResource.ConsentToCollectEthnicity.GetValueOrDefault() && !diversityResource.DateAndTimeEthnicityCollected.HasValue)
                results.Add(new ValidationResult("Date And Time Ethnicity Collected must have a value", new[] { "DateAndTimeEthnicityCollected" }));

            if (diversityResource.DateAndTimeEthnicityCollected.HasValue && diversityResource.DateAndTimeEthnicityCollected.Value > DateTime.UtcNow)
                results.Add(new ValidationResult("Date And Time Ethnicity Collected must be less the current date/time", new[] { "DateAndTimeEthnicityCollected" }));

            if (diversityResource.LastModifiedDate.HasValue && diversityResource.LastModifiedDate.Value > DateTime.UtcNow)
                results.Add(new ValidationResult("Last Modified Date must be less the current date/time", new[] { "LastModifiedDate" }));

            if (diversityResource.LearningDifficultyOrDisabilityDeclaration.HasValue && !Enum.IsDefined(typeof(LearningDifficultyOrDisabilityDeclaration), diversityResource.LearningDifficultyOrDisabilityDeclaration.Value))
                results.Add(new ValidationResult("Please supply a valid Learning Difficulty Or Disability Declaration", new[] { "LearningDifficultyOrDisabilityDeclaration" }));

            if (diversityResource.PrimaryLearningDifficultyOrDisability.HasValue && !Enum.IsDefined(typeof(PrimaryLearningDifficultyOrDisability), diversityResource.PrimaryLearningDifficultyOrDisability.Value))
                results.Add(new ValidationResult("Please supply a valid Primary Learning Difficulty Or Disability", new[] { "PrimaryLearningDifficultyOrDisability" }));

            if (diversityResource.SecondaryLearningDifficultyOrDisability.HasValue && !Enum.IsDefined(typeof(SecondaryLearningDifficultyOrDisability), diversityResource.SecondaryLearningDifficultyOrDisability.Value))
                results.Add(new ValidationResult("Please supply a valid Secondary Learning Difficulty Or Disability", new[] { "SecondaryLearningDifficultyOrDisability" }));

            if (diversityResource.Ethnicity.HasValue && !Enum.IsDefined(typeof(Ethnicity), diversityResource.Ethnicity.Value))
                results.Add(new ValidationResult("Please supply a valid Ethnicity", new[] { "Ethnicity" }));

        }

    }
}
