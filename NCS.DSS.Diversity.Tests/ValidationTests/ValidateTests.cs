using NCS.DSS.Diversity.ReferenceData;
using NCS.DSS.Diversity.Validation;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NCS.DSS.Diversity.Tests.ValidationTests
{
    public class ValidateTests
    {
        [Test]
        public void ValidateTests_ReturnValidationResult_WhenDiversityIsNotSuppliedForPost()
        {
            // Arrange
            var diversity = new Models.Diversity();
            var validation = new Validate();

            // Act
            var result = validation.ValidateResource(diversity);

            // Assert
            Assert.That(result, Is.InstanceOf<List<ValidationResult>>());
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(4));
        }

        [Test]
        public void ValidateTests_ReturnValidationResult_WhenConsentToCollectLLDDHealthIsNotSuppliedForPost()
        {
            // Arrange
            var validation = new Validate();
            var diversity = new Models.Diversity
            {
                LearningDifficultyOrDisabilityDeclaration = LearningDifficultyOrDisabilityDeclaration.NotProvidedByTheCustomer, //if anything else then an additional validation results will be returned for supplying something other than 'Not Provided By Customer' when consent is not true
                PrimaryLearningDifficultyOrDisability = PrimaryLearningDifficultyOrDisability.NotProvided, //if anything else then an additional validation results will be returned for supplying something other than 'Not Provided' when consent is not true
                SecondaryLearningDifficultyOrDisability = SecondaryLearningDifficultyOrDisability.NotProvided, //if anything else then an additional validation results will be returned for supplying something other than 'Not Provided' when consent is not true
                ConsentToCollectEthnicity = true,
                DateAndTimeEthnicityCollected = DateTime.UtcNow,
                Ethnicity = Ethnicity.AnyOtherEthnicGroup
            };

            // Act
            var result = validation.ValidateResource(diversity);

            // Assert
            Assert.That(result, Is.InstanceOf<List<ValidationResult>>());
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(1));
        }

        [Test]
        public void ValidateTests_ReturnValidationResult_WhenConsentToCollectEthnicityIsNotSuppliedForPost()
        {
            // Arrange
            var validation = new Validate();
            var diversity = new Models.Diversity
            {
                ConsentToCollectLLDDHealth = true,
                DateAndTimeLLDDHealthConsentCollected = DateTime.UtcNow,
                LearningDifficultyOrDisabilityDeclaration = LearningDifficultyOrDisabilityDeclaration.NotProvidedByTheCustomer,
                Ethnicity = Ethnicity.NotProvided //if anything else then an additional validation results will be returned for supplying something other than 'Not Provided' when consent is not true
            };

            // Act
            var result = validation.ValidateResource(diversity);

            // Assert
            Assert.That(result, Is.InstanceOf<List<ValidationResult>>());
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(1));
        }

        [Test]
        public void ValidateTests_ReturnValidationResult_WhenEthnicityIsNotSuppliedForPost()
        {
            // Arrange
            var validation = new Validate();
            var diversity = new Models.Diversity
            {
                ConsentToCollectLLDDHealth = true,
                DateAndTimeLLDDHealthConsentCollected = DateTime.UtcNow,
                LearningDifficultyOrDisabilityDeclaration = LearningDifficultyOrDisabilityDeclaration.NotProvidedByTheCustomer,
                ConsentToCollectEthnicity = true,
                DateAndTimeEthnicityCollected = DateTime.UtcNow
            };

            // Act
            var result = validation.ValidateResource(diversity);

            // Assert
            Assert.That(result, Is.InstanceOf<List<ValidationResult>>());
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(1));
        }

        [Test]
        public void ValidateTests_ReturnValidationResult_WhenLearningDifficultyOrDisabilityDeclarationIsNotSuppliedForPost()
        {
            // Arrange
            var validation = new Validate();
            var diversity = new Models.Diversity
            {
                ConsentToCollectLLDDHealth = true,
                DateAndTimeLLDDHealthConsentCollected = DateTime.UtcNow,
                ConsentToCollectEthnicity = true,
                DateAndTimeEthnicityCollected = DateTime.UtcNow,
                Ethnicity = Ethnicity.AnyOtherEthnicGroup
            };

            // Act
            var result = validation.ValidateResource(diversity);

            // Assert
            Assert.That(result, Is.InstanceOf<List<ValidationResult>>());
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(1));
        }

        [Test]
        public void ValidateTests_ReturnValidationResult_WhenDateAndTimeLLDDHealthConsentCollectedIsNotValid()
        {
            // Arrange
            var validation = new Validate();
            var diversity = new Models.Diversity
            {
                ConsentToCollectLLDDHealth = true,
                LearningDifficultyOrDisabilityDeclaration = LearningDifficultyOrDisabilityDeclaration.NotProvidedByTheCustomer,
                ConsentToCollectEthnicity = true,
                DateAndTimeEthnicityCollected = DateTime.UtcNow,
                Ethnicity = Ethnicity.AnyOtherEthnicGroup,
                DateAndTimeLLDDHealthConsentCollected = DateTime.MaxValue

            };

            // Act
            var result = validation.ValidateResource(diversity);

            // Assert
            Assert.That(result, Is.InstanceOf<List<ValidationResult>>());
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(1));
        }

        [Test]
        public void ValidateTests_ReturnValidationResult_WhenDateAndTimeEthnicityCollectedIsInTheFuture()
        {
            // Arrange
            var validation = new Validate();
            var diversity = new Models.Diversity
            {
                ConsentToCollectLLDDHealth = true,
                DateAndTimeLLDDHealthConsentCollected = DateTime.UtcNow,
                LearningDifficultyOrDisabilityDeclaration = LearningDifficultyOrDisabilityDeclaration.NotProvidedByTheCustomer,
                ConsentToCollectEthnicity = true,
                Ethnicity = Ethnicity.AnyOtherEthnicGroup,
                DateAndTimeEthnicityCollected = DateTime.MaxValue
            };

            // Act
            var result = validation.ValidateResource(diversity);

            // Assert
            Assert.That(result, Is.InstanceOf<List<ValidationResult>>());
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(1));
        }

        [Test]
        public void ValidateTests_ReturnValidationResult_WhenLastModifiedDateIsInTheFuture()
        {
            // Arrange
            var validation = new Validate();
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

            // Act
            var result = validation.ValidateResource(diversity);

            // Assert
            Assert.That(result, Is.InstanceOf<List<ValidationResult>>());
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(1));
        }

        [Test]
        public void ValidateTests_ReturnValidationResult_WhenLearningDifficultyOrDisabilityDeclarationIsNotValid()
        {
            // Arrange
            var validation = new Validate();
            var diversity = new Models.Diversity
            {
                ConsentToCollectLLDDHealth = true,
                DateAndTimeLLDDHealthConsentCollected = DateTime.UtcNow,
                LearningDifficultyOrDisabilityDeclaration = (LearningDifficultyOrDisabilityDeclaration)100,
                ConsentToCollectEthnicity = true,
                DateAndTimeEthnicityCollected = DateTime.UtcNow,
                Ethnicity = Ethnicity.AnyOtherEthnicGroup
            };

            // Act
            var result = validation.ValidateResource(diversity);

            // Assert
            Assert.That(result, Is.InstanceOf<List<ValidationResult>>());
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(1));
        }


        [Test]
        public void ValidateTests_ReturnValidationResult_WhenPrimaryLearningDifficultyOrDisabilityIsNotValid()
        {
            // Arrange
            var validation = new Validate();
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

            // Act
            var result = validation.ValidateResource(diversity);

            // Assert
            Assert.That(result, Is.InstanceOf<List<ValidationResult>>());
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(1));
        }

        [Test]
        public void ValidateTests_ReturnValidationResult_WhenSecondaryLearningDifficultyOrDisabilityIsNotValid()
        {
            // Arrange
            var validation = new Validate();
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

            // Act
            var result = validation.ValidateResource(diversity);

            // Assert
            Assert.That(result, Is.InstanceOf<List<ValidationResult>>());
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(1));
        }

        [Test]
        public void ValidateTests_ReturnValidationResult_WhenEthnicityIsNotValid()
        {
            // Arrange
            var validation = new Validate();
            var diversity = new Models.Diversity
            {
                ConsentToCollectLLDDHealth = true,
                DateAndTimeLLDDHealthConsentCollected = DateTime.UtcNow,
                LearningDifficultyOrDisabilityDeclaration = LearningDifficultyOrDisabilityDeclaration.NotProvidedByTheCustomer,
                ConsentToCollectEthnicity = true,
                DateAndTimeEthnicityCollected = DateTime.UtcNow,
                Ethnicity = (Ethnicity)100,
            };

            // Act
            var result = validation.ValidateResource(diversity);

            // Assert
            Assert.That(result, Is.InstanceOf<List<ValidationResult>>());
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(1));
        }

        [Test]
        public void ValidateTests_ReturnValidationResult_WhenLLDDHealthConsentIsFalseAndLearningDifficultyOrDisabilityDeclarationValueIsNotNotProvidedByCustomer()
        {
            // Arrange
            var validation = new Validate();
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

            // Act
            var result = validation.ValidateResource(diversity);

            // Assert
            Assert.That(result, Is.InstanceOf<List<ValidationResult>>());
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(1));
        }

        [Test]
        public void ValidateTests_ReturnValidationResult_WhenLLDDHealthConsentIsFalseAndPrimaryLearningDifficultyOrDisabilityValueIsNotNotProvided()
        {
            // Arrange
            var validation = new Validate();
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

            // Act
            var result = validation.ValidateResource(diversity);

            // Assert
            Assert.That(result, Is.InstanceOf<List<ValidationResult>>());
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(1));
        }

        [Test]
        public void ValidateTests_ReturnValidationResult_WhenLLDDHealthConsentIsFalseAndSecondaryLearningDifficultyOrDisabilityValueIsNotNotProvided()
        {
            // Arrange
            var validation = new Validate();
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

            // Act
            var result = validation.ValidateResource(diversity);

            // Assert
            Assert.That(result, Is.InstanceOf<List<ValidationResult>>());
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(1));
        }

        [Test]
        public void ValidateTests_ReturnValidationResult_WhenEthnicityConsentIsFalseAndEthnicityIsNotNotProvided()
        {
            // Arrange
            var validation = new Validate();
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

            // Act
            var result = validation.ValidateResource(diversity);

            // Assert
            Assert.That(result, Is.InstanceOf<List<ValidationResult>>());
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(1));
        }
    }
}
