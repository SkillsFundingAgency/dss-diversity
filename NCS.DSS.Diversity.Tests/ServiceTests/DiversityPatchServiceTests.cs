using DFC.JSON.Standard;
using Microsoft.Extensions.Logging;
using Moq;
using NCS.DSS.Diversity.Models;
using NCS.DSS.Diversity.PatchDiversityHttpTrigger.Service;
using NCS.DSS.Diversity.ReferenceData;
using Newtonsoft.Json;
using NUnit.Framework;
using System;

namespace NCS.DSS.Diversity.Tests.ServiceTests
{

    public class DiversityPatchServiceTests
    {
        private readonly JsonHelper _jsonHelper;
        private readonly IDiversityPatchService _diversityPatchService;
        private readonly DiversityPatch _diversityPatch;
        private readonly Mock<ILogger<DiversityPatchService>> _logger;
        private readonly string _json;

        public DiversityPatchServiceTests()
        {

            _jsonHelper = new JsonHelper();
            _logger = new Mock<ILogger<DiversityPatchService>>();
            _diversityPatchService = new DiversityPatchService(_jsonHelper, _logger.Object);
            _diversityPatch = new DiversityPatch();

            _json = JsonConvert.SerializeObject(_diversityPatch);
        }

        [Test]
        public void DiversityPatchServiceTests_ReturnsNull_WhenDiversityPatchNull()
        {
            // Act
            var result = _diversityPatchService.Patch(string.Empty, It.IsAny<DiversityPatch>());

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public void DiversityTests_CheckConsentToCollectLLDDHealthIsUpdated_WhenPatchIsCalled()
        {
            // Arrange
            var diversityPatch = new DiversityPatch { ConsentToCollectLLDDHealth = false };

            // Act
            var patchedDiversity = _diversityPatchService.Patch(_json, diversityPatch);
            var diversity = JsonConvert.DeserializeObject<Models.Diversity>(patchedDiversity);

            // Assert
            Assert.That(diversity.ConsentToCollectLLDDHealth, Is.False);
        }


        [Test]
        public void DiversityTests_CheckPrimaryLearningDifficultyOrDisabilityIsUpdated_WhenPatchIsCalled()
        {
            // Arrange
            var diversityPatch = new DiversityPatch { PrimaryLearningDifficultyOrDisability = PrimaryLearningDifficultyOrDisability.Dyslexia };

            // Act
            var patchedDiversity = _diversityPatchService.Patch(_json, diversityPatch);
            var diversity = JsonConvert.DeserializeObject<Models.Diversity>(patchedDiversity);

            // Assert
            Assert.That(diversity.PrimaryLearningDifficultyOrDisability, Is.EqualTo(PrimaryLearningDifficultyOrDisability.Dyslexia));
        }

        [Test]
        public void DiversityTests_CheckSecondaryLearningDifficultyOrDisabilityIsUpdated_WhenPatchIsCalled()
        {
            // Arrange
            var diversityPatch = new DiversityPatch { SecondaryLearningDifficultyOrDisability = SecondaryLearningDifficultyOrDisability.Dyslexia };

            // Act
            var patchedDiversity = _diversityPatchService.Patch(_json, diversityPatch);
            var diversity = JsonConvert.DeserializeObject<Models.Diversity>(patchedDiversity);

            // Assert
            Assert.That(diversity.SecondaryLearningDifficultyOrDisability, Is.EqualTo(SecondaryLearningDifficultyOrDisability.Dyslexia));
        }

        [Test]
        public void DiversityTests_CheckDateAndTimeLLDDHealthConsentCollectedIsUpdated_WhenPatchIsCalled()
        {
            // Arrange
            var diversityPatch = new DiversityPatch { DateAndTimeLLDDHealthConsentCollected = DateTime.MaxValue };

            // Act
            var patchedDiversity = _diversityPatchService.Patch(_json, diversityPatch);
            var diversity = JsonConvert.DeserializeObject<Models.Diversity>(patchedDiversity);

            // Assert
            Assert.That(diversity.DateAndTimeLLDDHealthConsentCollected, Is.EqualTo(DateTime.MaxValue));
        }

        [Test]
        public void DiversityTests_CheckConsentToCollectEthnicityIsUpdated_WhenPatchIsCalled()
        {
            // Arrange
            var diversityPatch = new DiversityPatch { ConsentToCollectEthnicity = false };

            // Act
            var patchedDiversity = _diversityPatchService.Patch(_json, diversityPatch);
            var diversity = JsonConvert.DeserializeObject<Models.Diversity>(patchedDiversity);

            // Assert
            Assert.That(diversity.ConsentToCollectEthnicity, Is.False);
        }

        [Test]
        public void DiversityTests_CheckEthnicityIsUpdated_WhenPatchIsCalled()
        {
            // Arrange
            var diversityPatch = new DiversityPatch { Ethnicity = Ethnicity.AnyOtherEthnicGroup };

            // Act
            var patchedDiversity = _diversityPatchService.Patch(_json, diversityPatch);
            var diversity = JsonConvert.DeserializeObject<Models.Diversity>(patchedDiversity);

            // Assert
            Assert.That(diversity.Ethnicity, Is.EqualTo(Ethnicity.AnyOtherEthnicGroup));
        }

        [Test]
        public void DiversityTests_CheckDateAndTimeEthnicityCollectedIsUpdated_WhenPatchIsCalled()
        {
            // Arrange
            var diversityPatch = new DiversityPatch { DateAndTimeEthnicityCollected = DateTime.MaxValue };

            // Act
            var patchedDiversity = _diversityPatchService.Patch(_json, diversityPatch);
            var diversity = JsonConvert.DeserializeObject<Models.Diversity>(patchedDiversity);

            // Assert
            Assert.That(diversity.DateAndTimeEthnicityCollected, Is.EqualTo(DateTime.MaxValue));
        }

        [Test]
        public void DiversityTests_CheckLastModifiedDateIsUpdated_WhenPatchIsCalled()
        {
            // Arrange
            var diversityPatch = new DiversityPatch { LastModifiedDate = DateTime.MaxValue };

            // Act
            var patchedDiversity = _diversityPatchService.Patch(_json, diversityPatch);
            var diversity = JsonConvert.DeserializeObject<Models.Diversity>(patchedDiversity);

            // Assert
            Assert.That(diversity.LastModifiedDate, Is.EqualTo(DateTime.MaxValue));
        }

        [Test]
        public void DiversityTests_CheckLastModifiedByUpdated_WhenPatchIsCalled()
        {
            // Arrange
            var diversityPatch = new DiversityPatch { LastModifiedBy = "0000000111" };

            // Act
            var patchedDiversity = _diversityPatchService.Patch(_json, diversityPatch);
            var diversity = JsonConvert.DeserializeObject<Models.Diversity>(patchedDiversity);

            // Assert
            Assert.That(diversity.LastModifiedBy, Is.EqualTo("0000000111"));
        }

    }
}