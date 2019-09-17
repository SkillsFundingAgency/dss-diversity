using System;
using NCS.DSS.Diversity.ReferenceData;
using NSubstitute;
using Xunit;

namespace NCS.DSS.Diversity.Tests.ModelTests
{
    public class DiversityTests
    {
        
        [Fact]
        public void DiversityTests_PopulatesDefaultValues_WhenSetDefaultValuesIsCalled()
        {
            var diversity = new Models.Diversity();
            diversity.SetDefaultValues();

            // Assert
            Assert.NotNull(diversity.LastModifiedDate);
            Assert.Equal(PrimaryLearningDifficultyOrDisability.NotProvided, diversity.PrimaryLearningDifficultyOrDisability);
            Assert.Equal(SecondaryLearningDifficultyOrDisability.NotProvided, diversity.SecondaryLearningDifficultyOrDisability);
        }

        [Fact]
        public void DiversityTests_CheckLastModifiedDateDoesNotGetPopulated_WhenSetDefaultValuesIsCalled()
        {
            var diversity = new Models.Diversity { LastModifiedDate = DateTime.MaxValue };

            diversity.SetDefaultValues();

            // Assert
            Assert.Equal(DateTime.MaxValue, diversity.LastModifiedDate);
        }
        
        [Fact]
        public void DiversityTests_CheckPrimaryLearningDifficultyOrDisabilityDoesNotGetPopulated_WhenSetDefaultValuesIsCalled()
        {
            var diversity = new Models.Diversity { PrimaryLearningDifficultyOrDisability = PrimaryLearningDifficultyOrDisability.Dyslexia };

            diversity.SetDefaultValues();

            // Assert
            Assert.Equal(PrimaryLearningDifficultyOrDisability.Dyslexia, diversity.PrimaryLearningDifficultyOrDisability);
        }

        [Fact]
        public void DiversityTests_CheckSecondaryLearningDifficultyOrDisabilityDoesNotGetPopulated_WhenSetDefaultValuesIsCalled()
        {
            var diversity = new Models.Diversity { SecondaryLearningDifficultyOrDisability = SecondaryLearningDifficultyOrDisability.Dyslexia };

            diversity.SetDefaultValues();

            // Assert
            Assert.Equal(SecondaryLearningDifficultyOrDisability.Dyslexia, diversity.SecondaryLearningDifficultyOrDisability);
        }
        [Fact]
        public void DiversityTests_CheckDiversityIdIsSet_WhenSetIdsIsCalled()
        {
            var diversity = new Models.Diversity();

            diversity.SetIds(Arg.Any<Guid>(), Arg.Any<string>());

            // Assert
            Assert.NotEqual(Guid.Empty, diversity.DiversityId);
        }

        [Fact]
        public void DiversityTests_CheckCustomerIdIsSet_WhenSetIdsIsCalled()
        {
            var diversity = new Models.Diversity();

            var customerId = Guid.NewGuid();
            diversity.SetIds(customerId, Arg.Any<string>());

            // Assert
            Assert.Equal(customerId, diversity.CustomerId);
        }

       [Fact]
        public void DiversityTests_CheckLastModifiedTouchpointIdIsSet_WhenSetIdsIsCalled()
        {
            var diversity = new Models.Diversity();

            diversity.SetIds(Arg.Any<Guid>(), "0000000000");

            // Assert
            Assert.Equal("0000000000", diversity.LastModifiedBy);
        }


    }
}
