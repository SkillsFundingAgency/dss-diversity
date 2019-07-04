using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using NCS.DSS.Diversity.ReferenceData;
using NCS.DSS.Diversity.Validation;
using NUnit.Framework;

namespace NCS.DSS.Diversity.Tests.ValidationTests
{
    [TestFixture]
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
            Assert.IsNotNull(result);
            Assert.AreEqual(4, result.Count);
        }

        [Test]
        public void ValidateTests_ReturnValidationResult_WhenConsentToCollectLLDDHealthIsNotSuppliedForPost()
        {
            var diversity = new Models.Diversity
            {
                LearningDifficultyOrDisabilityDeclaration = LearningDifficultyOrDisabilityDeclaration.NotProvidedByTheCustomer,
                ConsentToCollectEthnicity = true,
                Ethnicity = Ethnicity.AnyOtherEthnicGroup
            };

            var validation = new Validate();

            var result = validation.ValidateResource(diversity);

            // Assert
            Assert.IsInstanceOf<List<ValidationResult>>(result);
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
        }

        [Test]
        public void ValidateTests_ReturnValidationResult_WhenConsentToCollectEthnicityIsNotSuppliedForPost()
        {
            var diversity = new Models.Diversity
            {
                ConsentToCollectLLDDHealth = true,
                LearningDifficultyOrDisabilityDeclaration = LearningDifficultyOrDisabilityDeclaration.NotProvidedByTheCustomer,
                Ethnicity = Ethnicity.AnyOtherEthnicGroup
            };

            var validation = new Validate();

            var result = validation.ValidateResource(diversity);

            // Assert
            Assert.IsInstanceOf<List<ValidationResult>>(result);
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
        }

        [Test]
        public void ValidateTests_ReturnValidationResult_WhenEthnicityIsNotSuppliedForPost()
        {
            var diversity = new Models.Diversity
            {
                ConsentToCollectLLDDHealth = true,
                LearningDifficultyOrDisabilityDeclaration = LearningDifficultyOrDisabilityDeclaration.NotProvidedByTheCustomer,
                ConsentToCollectEthnicity = true,
            };

            var validation = new Validate();

            var result = validation.ValidateResource(diversity);

            // Assert
            Assert.IsInstanceOf<List<ValidationResult>>(result);
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
        }

        [Test]
        public void ValidateTests_ReturnValidationResult_WhenLearningDifficultyOrDisabilityDeclarationIsNotSuppliedForPost()
        {
            var diversity = new Models.Diversity
            {
                ConsentToCollectLLDDHealth = true,
                ConsentToCollectEthnicity = true,
                Ethnicity = Ethnicity.AnyOtherEthnicGroup
            };

            var validation = new Validate();

            var result = validation.ValidateResource(diversity);

            // Assert
            Assert.IsInstanceOf<List<ValidationResult>>(result);
            Assert.IsNotNull(result);
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
                Ethnicity = Ethnicity.AnyOtherEthnicGroup,
                DateAndTimeLLDDHealthConsentCollected = DateTime.MaxValue

            };

            var validation = new Validate();

            var result = validation.ValidateResource(diversity);

            // Assert
            Assert.IsInstanceOf<List<ValidationResult>>(result);
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
        }

        [Test]
        public void ValidateTests_ReturnValidationResult_WhenDateAndTimeEthnicityCollectedIsInTheFuture()
        {
            var diversity = new Models.Diversity
            {
                ConsentToCollectLLDDHealth = true,
                LearningDifficultyOrDisabilityDeclaration = LearningDifficultyOrDisabilityDeclaration.NotProvidedByTheCustomer,
                ConsentToCollectEthnicity = true,
                Ethnicity = Ethnicity.AnyOtherEthnicGroup,
                DateAndTimeEthnicityCollected = DateTime.MaxValue
            };

            var validation = new Validate();

            var result = validation.ValidateResource(diversity);

            // Assert
            Assert.IsInstanceOf<List<ValidationResult>>(result);
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
        }

        [Test]
        public void ValidateTests_ReturnValidationResult_WhenLastModifiedDateIsInTheFuture()
        {
            var diversity = new Models.Diversity
            {
                ConsentToCollectLLDDHealth = true,
                LearningDifficultyOrDisabilityDeclaration = LearningDifficultyOrDisabilityDeclaration.NotProvidedByTheCustomer,
                ConsentToCollectEthnicity = true,
                Ethnicity = Ethnicity.AnyOtherEthnicGroup,
                LastModifiedDate = DateTime.MaxValue
            };

            var validation = new Validate();

            var result = validation.ValidateResource(diversity);

            // Assert
            Assert.IsInstanceOf<List<ValidationResult>>(result);
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
        }

        [Test]
        public void ValidateTests_ReturnValidationResult_WhenLearningDifficultyOrDisabilityDeclarationIsNotValid()
        {
            var diversity = new Models.Diversity
            {
                ConsentToCollectLLDDHealth = true,
                LearningDifficultyOrDisabilityDeclaration = (LearningDifficultyOrDisabilityDeclaration)100,
                ConsentToCollectEthnicity = true,
                Ethnicity = Ethnicity.AnyOtherEthnicGroup
            };

            var validation = new Validate();

            var result = validation.ValidateResource(diversity);

            // Assert
            Assert.IsInstanceOf<List<ValidationResult>>(result);
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
        }


        [Test]
        public void ValidateTests_ReturnValidationResult_WhenPrimaryLearningDifficultyOrDisabilityIsNotValid()
        {
            var diversity = new Models.Diversity
            {
                ConsentToCollectLLDDHealth = true,
                LearningDifficultyOrDisabilityDeclaration = LearningDifficultyOrDisabilityDeclaration.NotProvidedByTheCustomer,
                PrimaryLearningDifficultyOrDisability = (PrimaryLearningDifficultyOrDisability)100,
                ConsentToCollectEthnicity = true,
                Ethnicity = Ethnicity.AnyOtherEthnicGroup
            };

            var validation = new Validate();

            var result = validation.ValidateResource(diversity);

            // Assert
            Assert.IsInstanceOf<List<ValidationResult>>(result);
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
        }

        [Test]
        public void ValidateTests_ReturnValidationResult_WhenSecondaryLearningDifficultyOrDisabilityIsNotValid()
        {
            var diversity = new Models.Diversity
            {
                ConsentToCollectLLDDHealth = true,
                LearningDifficultyOrDisabilityDeclaration = LearningDifficultyOrDisabilityDeclaration.NotProvidedByTheCustomer,
                SecondaryLearningDifficultyOrDisability = (SecondaryLearningDifficultyOrDisability)100,
                ConsentToCollectEthnicity = true,
                Ethnicity = Ethnicity.AnyOtherEthnicGroup
            };

            var validation = new Validate();

            var result = validation.ValidateResource(diversity);

            // Assert
            Assert.IsInstanceOf<List<ValidationResult>>(result);
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
        }

        [Test]
        public void ValidateTests_ReturnValidationResult_WhenEthnicityIsNotValid()
        {
            var diversity = new Models.Diversity
            {
                ConsentToCollectLLDDHealth = true,
                LearningDifficultyOrDisabilityDeclaration = LearningDifficultyOrDisabilityDeclaration.NotProvidedByTheCustomer,
                ConsentToCollectEthnicity = true,
                Ethnicity = (Ethnicity)100,
            };

            var validation = new Validate();

            var result = validation.ValidateResource(diversity);

            // Assert
            Assert.IsInstanceOf<List<ValidationResult>>(result);
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
        }
    }
}
