using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.Azure.Cosmos.Serialization.HybridRow;
using Microsoft.Azure.Documents;
using System.Reflection.Metadata;
using NCS.DSS.Diversity.ReferenceData;
using NCS.DSS.Diversity.Validation;
using NUnit.Framework;

namespace NCS.DSS.Diversity.Tests.ValidationTests
{
   
    public class ValidateTests
    {
        
        [Test]
        public void ValidateTests_ReturnValidationResult_WhenDiversityIsNotSuppliedForPost()
        {
            var diversity = new Models.Diversity();

            var validation = new Validate();

            var result = validation.ValidateResource(diversity);

            // Assert
            Assert.IsInstanceOf<List<ValidationResult>>(result);
            Assert.NotNull(result);
            Assert.AreEqual(8, result.Count);
        }

        [Test]
        public void ValidateTests_ReturnValidationResult_WhenConsentToCollectLLDDHealthIsNotSuppliedForPost()
        {
            var diversity = new Models.Diversity
            {
            LearningDifficultyOrDisabilityDeclaration = LearningDifficultyOrDisabilityDeclaration.NotProvidedByTheCustomer, //if anything else then an additional validation results will be returned for supplying something other than 'Not Provided By Customer' when consent is not true
            PrimaryLearningDifficultyOrDisability = PrimaryLearningDifficultyOrDisability.NotProvided, //if anything else then an additional validation results will be returned for supplying something other than 'Not Provided' when consent is not true
            SecondaryLearningDifficultyOrDisability = SecondaryLearningDifficultyOrDisability.NotProvided, //if anything else then an additional validation results will be returned for supplying something other than 'Not Provided' when consent is not true
            ConsentToCollectEthnicity = true,
            DateAndTimeEthnicityCollected = DateTime.UtcNow,
            Ethnicity = Ethnicity.AnyOtherEthnicGroup
            }; 

            var validation = new Validate();

            var result = validation.ValidateResource(diversity);

            // Assert
            Assert.IsInstanceOf<List<ValidationResult>>(result);
            Assert.NotNull(result);
            Assert.AreEqual(1, result.Count);
        }

        [Test]
        public void ValidateTests_ReturnValidationResult_WhenConsentToCollectEthnicityIsNotSuppliedForPost()
        {
            var diversity = new Models.Diversity
            {
                ConsentToCollectLLDDHealth = true,
                DateAndTimeLLDDHealthConsentCollected = DateTime.UtcNow,
                LearningDifficultyOrDisabilityDeclaration = LearningDifficultyOrDisabilityDeclaration.NotProvidedByTheCustomer,
                Ethnicity = Ethnicity.NotProvided //if anything else then an additional validation results will be returned for supplying something other than 'Not Provided' when consent is not true
            };

            var validation = new Validate();

            var result = validation.ValidateResource(diversity);

            // Assert
            Assert.IsInstanceOf<List<ValidationResult>>(result);
            Assert.NotNull(result);
            Assert.AreEqual(1, result.Count);
        }

        [Test]
        public void ValidateTests_ReturnValidationResult_WhenEthnicityIsNotSuppliedForPost()
        {
            var diversity = new Models.Diversity
            {
                ConsentToCollectLLDDHealth = true,
                DateAndTimeLLDDHealthConsentCollected = DateTime.UtcNow,
                LearningDifficultyOrDisabilityDeclaration = LearningDifficultyOrDisabilityDeclaration.NotProvidedByTheCustomer,
                ConsentToCollectEthnicity = true,
                DateAndTimeEthnicityCollected = DateTime.UtcNow
            };

            var validation = new Validate();

            var result = validation.ValidateResource(diversity);

            // Assert
            Assert.IsInstanceOf<List<ValidationResult>>(result);
            Assert.NotNull(result);
            Assert.AreEqual(1, result.Count);
        }

        [Test]
        public void ValidateTests_ReturnValidationResult_WhenLearningDifficultyOrDisabilityDeclarationIsNotSuppliedForPost()
        {
            var diversity = new Models.Diversity
            {
                ConsentToCollectLLDDHealth = true,
                DateAndTimeLLDDHealthConsentCollected = DateTime.UtcNow,
                ConsentToCollectEthnicity = true,
                DateAndTimeEthnicityCollected = DateTime.UtcNow,
                Ethnicity = Ethnicity.AnyOtherEthnicGroup
            };

            var validation = new Validate();

            var result = validation.ValidateResource(diversity);

            // Assert
            Assert.IsInstanceOf<List<ValidationResult>>(result);
            Assert.NotNull(result);
            Assert.AreEqual(1, result.Count);
        }

        [Test]
        public void ValidateTests_ReturnValidationResult_WhenDateAndTimeLLDDHealthConsentCollectedIsNotValid()
        {
            var diversity = new Models.Diversity
            {
                ConsentToCollectLLDDHealth = true,
                LearningDifficultyOrDisabilityDeclaration = LearningDifficultyOrDisabilityDeclaration.NotProvidedByTheCustomer,
                ConsentToCollectEthnicity = true,
                DateAndTimeEthnicityCollected = DateTime.UtcNow,
                Ethnicity = Ethnicity.AnyOtherEthnicGroup,
                DateAndTimeLLDDHealthConsentCollected = DateTime.MaxValue

            };

            var validation = new Validate();

            var result = validation.ValidateResource(diversity);

            // Assert
            Assert.IsInstanceOf<List<ValidationResult>>(result);
            Assert.NotNull(result);
            Assert.AreEqual(1, result.Count);
        }

        [Test]
        public void ValidateTests_ReturnValidationResult_WhenDateAndTimeEthnicityCollectedIsInTheFuture()
        {
            var diversity = new Models.Diversity
            {
                ConsentToCollectLLDDHealth = true,
                DateAndTimeLLDDHealthConsentCollected = DateTime.UtcNow,
                LearningDifficultyOrDisabilityDeclaration = LearningDifficultyOrDisabilityDeclaration.NotProvidedByTheCustomer,
                ConsentToCollectEthnicity = true,
                Ethnicity = Ethnicity.AnyOtherEthnicGroup,
                DateAndTimeEthnicityCollected = DateTime.MaxValue
            };

            var validation = new Validate();

            var result = validation.ValidateResource(diversity);

            // Assert
            Assert.IsInstanceOf<List<ValidationResult>>(result);
            Assert.NotNull(result);
            Assert.AreEqual(1, result.Count);
        }

        [Test]
        public void ValidateTests_ReturnValidationResult_WhenLastModifiedDateIsInTheFuture()
        {
            var diversity = new Models.Diversity
            {
                ConsentToCollectLLDDHealth = true,
                DateAndTimeLLDDHealthConsentCollected = DateTime.UtcNow,
                LearningDifficultyOrDisabilityDeclaration = LearningDifficultyOrDisabilityDeclaration.NotProvidedByTheCustomer,
                ConsentToCollectEthnicity = true,
                DateAndTimeEthnicityCollected = DateTime.UtcNow,
                Ethnicity = Ethnicity.AnyOtherEthnicGroup,
                LastModifiedDate = DateTime.MaxValue
            };

            var validation = new Validate();

            var result = validation.ValidateResource(diversity);

            // Assert
            Assert.IsInstanceOf<List<ValidationResult>>(result);
            Assert.NotNull(result);
            Assert.AreEqual(1, result.Count);
        }

        [Test]
        public void ValidateTests_ReturnValidationResult_WhenLearningDifficultyOrDisabilityDeclarationIsNotValid()
        {
            var diversity = new Models.Diversity
            {
                ConsentToCollectLLDDHealth = true,
                DateAndTimeLLDDHealthConsentCollected = DateTime.UtcNow,
                LearningDifficultyOrDisabilityDeclaration = (LearningDifficultyOrDisabilityDeclaration)100,
                ConsentToCollectEthnicity = true,
                DateAndTimeEthnicityCollected = DateTime.UtcNow,
                Ethnicity = Ethnicity.AnyOtherEthnicGroup
            };

            var validation = new Validate();

            var result = validation.ValidateResource(diversity);

            // Assert
            Assert.IsInstanceOf<List<ValidationResult>>(result);
            Assert.NotNull(result);
            Assert.AreEqual(1, result.Count);
        }


        [Test]
        public void ValidateTests_ReturnValidationResult_WhenPrimaryLearningDifficultyOrDisabilityIsNotValid()
        {
            var diversity = new Models.Diversity
            {
                ConsentToCollectLLDDHealth = true,
                DateAndTimeLLDDHealthConsentCollected = DateTime.UtcNow,
                LearningDifficultyOrDisabilityDeclaration = LearningDifficultyOrDisabilityDeclaration.NotProvidedByTheCustomer,
                PrimaryLearningDifficultyOrDisability = (PrimaryLearningDifficultyOrDisability)100,
                ConsentToCollectEthnicity = true,
                DateAndTimeEthnicityCollected = DateTime.UtcNow,
                Ethnicity = Ethnicity.AnyOtherEthnicGroup
            };

            var validation = new Validate();

            var result = validation.ValidateResource(diversity);

            // Assert
            Assert.IsInstanceOf<List<ValidationResult>>(result);
            Assert.AreEqual(1, result.Count);
        }

        [Test]
        public void ValidateTests_ReturnValidationResult_WhenSecondaryLearningDifficultyOrDisabilityIsNotValid()
        {
            var diversity = new Models.Diversity
            {
                ConsentToCollectLLDDHealth = true,
                DateAndTimeLLDDHealthConsentCollected = DateTime.UtcNow,
                LearningDifficultyOrDisabilityDeclaration = LearningDifficultyOrDisabilityDeclaration.NotProvidedByTheCustomer,
                SecondaryLearningDifficultyOrDisability = (SecondaryLearningDifficultyOrDisability)100,
                ConsentToCollectEthnicity = true,
                DateAndTimeEthnicityCollected = DateTime.UtcNow,
                Ethnicity = Ethnicity.AnyOtherEthnicGroup
            };

            var validation = new Validate();

            var result = validation.ValidateResource(diversity);

            // Assert
            Assert.IsInstanceOf<List<ValidationResult>>(result);
            Assert.NotNull(result);
            Assert.AreEqual(1, result.Count);
        }

        [Test]
        public void ValidateTests_ReturnValidationResult_WhenEthnicityIsNotValid()
        {
            var diversity = new Models.Diversity
            {
                ConsentToCollectLLDDHealth = true,
                DateAndTimeLLDDHealthConsentCollected = DateTime.UtcNow,
                LearningDifficultyOrDisabilityDeclaration = LearningDifficultyOrDisabilityDeclaration.NotProvidedByTheCustomer,
                ConsentToCollectEthnicity = true,
                DateAndTimeEthnicityCollected = DateTime.UtcNow,
                Ethnicity = (Ethnicity)100,
            };

            var validation = new Validate();

            var result = validation.ValidateResource(diversity);

            // Assert
            Assert.IsInstanceOf<List<ValidationResult>>(result);
            Assert.NotNull(result);
            Assert.AreEqual(1, result.Count);
        }

        [Test]
        public void ValidateTests_ReturnValidationResult_WhenLLDDHealthConsentIsFalseAndLearningDifficultyOrDisabilityDeclarationValueIsNotNotProvidedByCustomer()
        {
            var diversity = new Models.Diversity
            {
                ConsentToCollectLLDDHealth = false,
                DateAndTimeLLDDHealthConsentCollected = DateTime.UtcNow,
                LearningDifficultyOrDisabilityDeclaration = LearningDifficultyOrDisabilityDeclaration.CustomerConsidersThemselvesToHaveALearningDifficultyAndOrHealthProblem,
                PrimaryLearningDifficultyOrDisability = PrimaryLearningDifficultyOrDisability.NotProvided, 
                SecondaryLearningDifficultyOrDisability = SecondaryLearningDifficultyOrDisability.NotProvided,
                ConsentToCollectEthnicity = true,
                DateAndTimeEthnicityCollected = DateTime.UtcNow,
                Ethnicity = Ethnicity.EnglishWelshScottishNorthernIrishBritish,
            };

            var validation = new Validate();

            var result = validation.ValidateResource(diversity);

            // Assert
            Assert.IsInstanceOf<List<ValidationResult>>(result);
            Assert.NotNull(result);
            Assert.AreEqual(1, result.Count);
        }

        [Test]
        public void ValidateTests_ReturnValidationResult_WhenLLDDHealthConsentIsFalseAndPrimaryLearningDifficultyOrDisabilityValueIsNotNotProvided()
        {
            var diversity = new Models.Diversity
            {
                ConsentToCollectLLDDHealth = false,
                DateAndTimeLLDDHealthConsentCollected = DateTime.UtcNow,
                LearningDifficultyOrDisabilityDeclaration = LearningDifficultyOrDisabilityDeclaration.NotProvidedByTheCustomer,
                PrimaryLearningDifficultyOrDisability = PrimaryLearningDifficultyOrDisability.VisualImpairment,
                SecondaryLearningDifficultyOrDisability = SecondaryLearningDifficultyOrDisability.NotProvided,
                ConsentToCollectEthnicity = true,
                DateAndTimeEthnicityCollected = DateTime.UtcNow,
                Ethnicity = Ethnicity.EnglishWelshScottishNorthernIrishBritish,
            };

            var validation = new Validate();

            var result = validation.ValidateResource(diversity);

            // Assert
            Assert.IsInstanceOf<List<ValidationResult>>(result);
            Assert.NotNull(result);
            Assert.AreEqual(1, result.Count);
        }

        [Test]
        public void ValidateTests_ReturnValidationResult_WhenLLDDHealthConsentIsFalseAndSecondaryLearningDifficultyOrDisabilityValueIsNotNotProvided()
        {
            var diversity = new Models.Diversity
            {
                ConsentToCollectLLDDHealth = false,
                DateAndTimeLLDDHealthConsentCollected = DateTime.UtcNow,
                LearningDifficultyOrDisabilityDeclaration = LearningDifficultyOrDisabilityDeclaration.NotProvidedByTheCustomer,
                PrimaryLearningDifficultyOrDisability = PrimaryLearningDifficultyOrDisability.NotProvided,
                SecondaryLearningDifficultyOrDisability = SecondaryLearningDifficultyOrDisability.VisualImpairment,
                ConsentToCollectEthnicity = true,
                DateAndTimeEthnicityCollected = DateTime.UtcNow,
                Ethnicity = Ethnicity.EnglishWelshScottishNorthernIrishBritish,
            };

            var validation = new Validate();

            var result = validation.ValidateResource(diversity);

            // Assert
            Assert.IsInstanceOf<List<ValidationResult>>(result);
            Assert.NotNull(result);
            Assert.AreEqual(1, result.Count);
        }

        [Test]
        public void ValidateTests_ReturnValidationResult_WhenEthnicityConsentIsFalseAndEthnicityIsNotNotProvided()
        {
            var diversity = new Models.Diversity
            {
                ConsentToCollectLLDDHealth = true,
                DateAndTimeLLDDHealthConsentCollected = DateTime.UtcNow,
                LearningDifficultyOrDisabilityDeclaration = LearningDifficultyOrDisabilityDeclaration.NotProvidedByTheCustomer,
                PrimaryLearningDifficultyOrDisability = PrimaryLearningDifficultyOrDisability.NotProvided,
                SecondaryLearningDifficultyOrDisability = SecondaryLearningDifficultyOrDisability.NotProvided,
                ConsentToCollectEthnicity = false,
                DateAndTimeEthnicityCollected = DateTime.UtcNow,
                Ethnicity = Ethnicity.EnglishWelshScottishNorthernIrishBritish,
            };

            var validation = new Validate();

            var result = validation.ValidateResource(diversity);

            // Assert
            Assert.IsInstanceOf<List<ValidationResult>>(result);
            Assert.NotNull(result);
            Assert.AreEqual(1, result.Count);
        }
    }
}
