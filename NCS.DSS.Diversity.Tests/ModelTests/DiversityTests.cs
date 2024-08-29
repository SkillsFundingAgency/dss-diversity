using Moq;
using NCS.DSS.Diversity.ReferenceData;
using NUnit.Framework;
using System;

namespace NCS.DSS.Diversity.Tests.ModelTests
{
    public class DiversityTests
    {

        [Test]
        public void DiversityTests_PopulatesDefaultValues_WhenSetDefaultValuesIsCalled()
        {
            // Arrange
            var diversity = new Models.Diversity();

            // Act
            diversity.SetDefaultValues();

            // Assert
            Assert.That(diversity.LastModifiedDate, Is.Not.Null);
            Assert.That(diversity.PrimaryLearningDifficultyOrDisability, Is.EqualTo(PrimaryLearningDifficultyOrDisability.NotProvided));
            Assert.That(diversity.SecondaryLearningDifficultyOrDisability, Is.EqualTo(SecondaryLearningDifficultyOrDisability.NotProvided));
        }

        [Test]
        public void DiversityTests_CheckLastModifiedDateDoesNotGetPopulated_WhenSetDefaultValuesIsCalled()
        {
            // Arrange
            var diversity = new Models.Diversity { LastModifiedDate = DateTime.MaxValue };

            // Act
            diversity.SetDefaultValues();

            // Assert
            Assert.That(diversity.LastModifiedDate, Is.EqualTo(DateTime.MaxValue));
        }

        [Test]
        public void DiversityTests_CheckPrimaryLearningDifficultyOrDisabilityDoesNotGetPopulated_WhenSetDefaultValuesIsCalled()
        {
            // Arrange
            var diversity = new Models.Diversity { PrimaryLearningDifficultyOrDisability = PrimaryLearningDifficultyOrDisability.Dyslexia };

            // Act
            diversity.SetDefaultValues();

            // Assert
            Assert.That(diversity.PrimaryLearningDifficultyOrDisability, Is.EqualTo(PrimaryLearningDifficultyOrDisability.Dyslexia));
        }

        [Test]
        public void DiversityTests_CheckSecondaryLearningDifficultyOrDisabilityDoesNotGetPopulated_WhenSetDefaultValuesIsCalled()
        {
            // Arrange
            var diversity = new Models.Diversity { SecondaryLearningDifficultyOrDisability = SecondaryLearningDifficultyOrDisability.Dyslexia };

            // Act
            diversity.SetDefaultValues();

            // Assert
            Assert.That(diversity.SecondaryLearningDifficultyOrDisability, Is.EqualTo(SecondaryLearningDifficultyOrDisability.Dyslexia));
        }
        [Test]
        public void DiversityTests_CheckDiversityIdIsSet_WhenSetIdsIsCalled()
        {
            var diversity = new Models.Diversity();

            diversity.SetIds(It.IsAny<Guid>(), It.IsAny<string>());

            // Assert
            Assert.That(diversity.DiversityId, Is.Not.EqualTo(Guid.Empty));
        }

        [Test]
        public void DiversityTests_CheckCustomerIdIsSet_WhenSetIdsIsCalled()
        {
            var diversity = new Models.Diversity();

            var customerId = Guid.NewGuid();
            diversity.SetIds(customerId, It.IsAny<string>());

            // Assert
            Assert.That(diversity.CustomerId, Is.EqualTo(customerId));
        }

        [Test]
        public void DiversityTests_CheckLastModifiedTouchpointIdIsSet_WhenSetIdsIsCalled()
        {
            var diversity = new Models.Diversity();

            diversity.SetIds(It.IsAny<Guid>(), "0000000000");

            // Assert
            Assert.That(diversity.LastModifiedBy, Is.EqualTo("0000000000"));
        }


    }
}
