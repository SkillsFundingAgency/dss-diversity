using System;
using DFC.JSON.Standard;
using NCS.DSS.Diversity.Models;
using NCS.DSS.Diversity.PatchDiversityHttpTrigger.Service;
using NCS.DSS.Diversity.ReferenceData;
using Newtonsoft.Json;
using Moq;
using NUnit.Framework;

namespace NCS.DSS.Diversity.Tests.ServiceTests
{
   
    public class DiversityPatchServiceTests
    {
        private readonly JsonHelper _jsonHelper;
        private readonly IDiversityPatchService _diversityPatchService;
        private readonly DiversityPatch _diversityPatch;
        private readonly string _json;

        public DiversityPatchServiceTests()
        {

            _jsonHelper = new JsonHelper();
            _diversityPatchService = new DiversityPatchService(_jsonHelper);
            _diversityPatch = new Models.DiversityPatch();

            _json = JsonConvert.SerializeObject(_diversityPatch);
        }

        [Test]
        public void DiversityPatchServiceTests_ReturnsNull_WhenDiversityPatchNull()
        {
            var result = _diversityPatchService.Patch(string.Empty, It.IsAny<DiversityPatch>());

            // Assert
            Assert.Null(result);
        }

        [Test]
        public void DiversityTests_CheckConsentToCollectLLDDHealthIsUpdated_WhenPatchIsCalled()
        {
            var diversityPatch = new DiversityPatch { ConsentToCollectLLDDHealth = false };

            var patchedDiversity = _diversityPatchService.Patch(_json, diversityPatch);

            var diversity = JsonConvert.DeserializeObject<Models.Diversity>(patchedDiversity);

            // Assert
            Assert.AreEqual(false, diversity.ConsentToCollectLLDDHealth);
        }


        [Test]
        public void DiversityTests_CheckPrimaryLearningDifficultyOrDisabilityIsUpdated_WhenPatchIsCalled()
        {
            var diversityPatch = new Models.DiversityPatch { PrimaryLearningDifficultyOrDisability = PrimaryLearningDifficultyOrDisability.Dyslexia };

            var patchedDiversity = _diversityPatchService.Patch(_json, diversityPatch);

            var diversity = JsonConvert.DeserializeObject<Models.Diversity>(patchedDiversity);

            // Assert
            Assert.AreEqual(PrimaryLearningDifficultyOrDisability.Dyslexia, diversity.PrimaryLearningDifficultyOrDisability);
        }

        [Test]
        public void DiversityTests_CheckSecondaryLearningDifficultyOrDisabilityIsUpdated_WhenPatchIsCalled()
        {
            var diversityPatch = new Models.DiversityPatch { SecondaryLearningDifficultyOrDisability = SecondaryLearningDifficultyOrDisability.Dyslexia };

            var patchedDiversity = _diversityPatchService.Patch(_json, diversityPatch);

            var diversity = JsonConvert.DeserializeObject<Models.Diversity>(patchedDiversity);

            // Assert
            Assert.AreEqual(SecondaryLearningDifficultyOrDisability.Dyslexia, diversity.SecondaryLearningDifficultyOrDisability);
        }

        [Test]
        public void DiversityTests_CheckDateAndTimeLLDDHealthConsentCollectedIsUpdated_WhenPatchIsCalled()
        {
            var diversityPatch = new Models.DiversityPatch { DateAndTimeLLDDHealthConsentCollected = DateTime.MaxValue };

            var patchedDiversity = _diversityPatchService.Patch(_json, diversityPatch);

            var diversity = JsonConvert.DeserializeObject<Models.Diversity>(patchedDiversity);

            // Assert
            Assert.AreEqual(DateTime.MaxValue, diversity.DateAndTimeLLDDHealthConsentCollected);
        }

        [Test]
        public void DiversityTests_CheckConsentToCollectEthnicityIsUpdated_WhenPatchIsCalled()
        {
            var diversityPatch = new Models.DiversityPatch { ConsentToCollectEthnicity = false };

            var patchedDiversity = _diversityPatchService.Patch(_json, diversityPatch);

            var diversity = JsonConvert.DeserializeObject<Models.Diversity>(patchedDiversity);

            // Assert
            Assert.AreEqual(false, diversity.ConsentToCollectEthnicity);
        }

        [Test]
        public void DiversityTests_CheckEthnicityIsUpdated_WhenPatchIsCalled()
        {
            var diversityPatch = new Models.DiversityPatch { Ethnicity = Ethnicity.AnyOtherEthnicGroup };

            var patchedDiversity = _diversityPatchService.Patch(_json, diversityPatch);

            var diversity = JsonConvert.DeserializeObject<Models.Diversity>(patchedDiversity);

            // Assert
            Assert.AreEqual(Ethnicity.AnyOtherEthnicGroup, diversity.Ethnicity);
        }

        [Test]
        public void DiversityTests_CheckDateAndTimeEthnicityCollectedIsUpdated_WhenPatchIsCalled()
        {
            var diversityPatch = new Models.DiversityPatch { DateAndTimeEthnicityCollected = DateTime.MaxValue };

            var patchedDiversity = _diversityPatchService.Patch(_json, diversityPatch);

            var diversity = JsonConvert.DeserializeObject<Models.Diversity>(patchedDiversity);

            // Assert
            Assert.AreEqual(DateTime.MaxValue, diversity.DateAndTimeEthnicityCollected);
        }

        [Test]
        public void DiversityTests_CheckLastModifiedDateIsUpdated_WhenPatchIsCalled()
        {
            var diversityPatch = new Models.DiversityPatch { LastModifiedDate = DateTime.MaxValue };

            var patchedDiversity = _diversityPatchService.Patch(_json, diversityPatch);

            var diversity = JsonConvert.DeserializeObject<Models.Diversity>(patchedDiversity);

            // Assert
            Assert.AreEqual(DateTime.MaxValue, diversity.LastModifiedDate);
        }

        [Test]
        public void DiversityTests_CheckLastModifiedByUpdated_WhenPatchIsCalled()
        {
            var diversityPatch = new Models.DiversityPatch { LastModifiedBy = "0000000111" };

            var patchedDiversity = _diversityPatchService.Patch(_json, diversityPatch);

            var diversity = JsonConvert.DeserializeObject<Models.Diversity>(patchedDiversity);

            // Assert
            Assert.AreEqual("0000000111", diversity.LastModifiedBy);
        }
        
    }
}