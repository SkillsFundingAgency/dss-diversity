﻿using System;
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

            if (diversityResource.DateAndTimeLLDDHealthConsentCollected.HasValue && diversityResource.DateAndTimeLLDDHealthConsentCollected.Value > DateTime.UtcNow)
                results.Add(new ValidationResult("Date And Time LLDD Health Consent Collected must be less the current date/time", new[] { "DateAndTimeLLDDHealthConsentCollected" }));

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
             
            if (diversityResource.ConsentToCollectLLDDHealth != true && !diversityResource.LearningDifficultyOrDisabilityDeclaration.Equals(LearningDifficultyOrDisabilityDeclaration.NotProvidedByTheCustomer))
                results.Add(new ValidationResult("Learning Difficulty Or Disability Declaration must not be provided when no consent has been given to collect LLDD Health", new[] { "LearningDifficultyOrDisabilityDeclaration" }));

            if (diversityResource.ConsentToCollectLLDDHealth != true && (!diversityResource.PrimaryLearningDifficultyOrDisability.Equals(PrimaryLearningDifficultyOrDisability.PreferNotToSay) && !diversityResource.PrimaryLearningDifficultyOrDisability.Equals(PrimaryLearningDifficultyOrDisability.NotProvided)))
                results.Add(new ValidationResult("Primary Learning Difficulty Or Disability must not be provided when no consent has been given to collect LLDD Health", new[] { "PrimaryLearningDifficultyOrDisability" }));

            if (diversityResource.ConsentToCollectLLDDHealth != true && (!diversityResource.SecondaryLearningDifficultyOrDisability.Equals(SecondaryLearningDifficultyOrDisability.PreferNotToSay) && !diversityResource.SecondaryLearningDifficultyOrDisability.Equals(SecondaryLearningDifficultyOrDisability.NotProvided)))
                results.Add(new ValidationResult("Secondary Learning Difficulty Or Disability must not be provided when no consent has been given to collect LLDD Health", new[] { "SecondaryLearningDifficultyOrDisability" }));

            if (diversityResource.ConsentToCollectEthnicity != true && !diversityResource.Ethnicity.Equals(Ethnicity.NotProvided))
                results.Add(new ValidationResult("Ethnicity must not be provided when no consent has been given to collect Ethnicity", new[] { "Ethnicity" }));

        }

    }
}
