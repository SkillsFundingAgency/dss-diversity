using System;
using DFC.JSON.Standard;
using NCS.DSS.Diversity.Models;
using NCS.DSS.Diversity.PatchDiversityHttpTrigger.Service;
using NCS.DSS.Diversity.ReferenceData;
using Newtonsoft.Json;
using NSubstitute;
using Xunit;

namespace NCS.DSS.Diversity.Tests.ServiceTests
{
   
    public class DiversityPatchServiceTests
    {
        private readonly IJsonHelper _jsonHelper;
        private readonly IDiversityPatchService _diversityPatchService;
        private readonly DiversityPatch _diversityPatch;
        private readonly string _json;

        public DiversityPatchServiceTests()
        {

            _jsonHelper = Substitute.For<JsonHelper>();
            _diversityPatchService = Substitute.For<DiversityPatchService>(_jsonHelper);
            _diversityPatch = Substitute.For<DiversityPatch>();

            _json = JsonConvert.SerializeObject(_diversityPatch);
        }

        [Fact]
        public void DiversityPatchServiceTests_ReturnsNull_WhenDiversityPatchNull()
        {
            var result = _diversityPatchService.Patch(string.Empty, Arg.Any<DiversityPatch>());

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void DiversityTests_CheckConsentToCollectLLDDHealthIsUpdated_WhenPatchIsCalled()
        {
            var diversityPatch = new DiversityPatch { ConsentToCollectLLDDHealth = false };

            var patchedDiversity = _diversityPatchService.Patch(_json, diversityPatch);

            var diversity = JsonConvert.DeserializeObject<Models.Diversity>(patchedDiversity);

            // Assert
            Assert.Equal(false, diversity.ConsentToCollectLLDDHealth);
        }


        [Fact]
        public void DiversityTests_CheckPrimaryLearningDifficultyOrDisabilityIsUpdated_WhenPatchIsCalled()
        {
            var diversityPatch = new Models.DiversityPatch { PrimaryLearningDifficultyOrDisability = PrimaryLearningDifficultyOrDisability.Dyslexia };

            var patchedDiversity = _diversityPatchService.Patch(_json, diversityPatch);

            var diversity = JsonConvert.DeserializeObject<Models.Diversity>(patchedDiversity);

            // Assert
            Assert.Equal(PrimaryLearningDifficultyOrDisability.Dyslexia, diversity.PrimaryLearningDifficultyOrDisability);
        }

        [Fact]
        public void DiversityTests_CheckSecondaryLearningDifficultyOrDisabilityIsUpdated_WhenPatchIsCalled()
        {
            var diversityPatch = new Models.DiversityPatch { SecondaryLearningDifficultyOrDisability = SecondaryLearningDifficultyOrDisability.Dyslexia };

            var patchedDiversity = _diversityPatchService.Patch(_json, diversityPatch);

            var diversity = JsonConvert.DeserializeObject<Models.Diversity>(patchedDiversity);

            // Assert
            Assert.Equal(SecondaryLearningDifficultyOrDisability.Dyslexia, diversity.SecondaryLearningDifficultyOrDisability);
        }

        [Fact]
        public void DiversityTests_CheckDateAndTimeLLDDHealthConsentCollectedIsUpdated_WhenPatchIsCalled()
        {
            var diversityPatch = new Models.DiversityPatch { DateAndTimeLLDDHealthConsentCollected = DateTime.MaxValue };

            var patchedDiversity = _diversityPatchService.Patch(_json, diversityPatch);

            var diversity = JsonConvert.DeserializeObject<Models.Diversity>(patchedDiversity);

            // Assert
            Assert.Equal(DateTime.MaxValue, diversity.DateAndTimeLLDDHealthConsentCollected);
        }

        [Fact]
        public void DiversityTests_CheckConsentToCollectEthnicityIsUpdated_WhenPatchIsCalled()
        {
            var diversityPatch = new Models.DiversityPatch { ConsentToCollectEthnicity = false };

            var patchedDiversity = _diversityPatchService.Patch(_json, diversityPatch);

            var diversity = JsonConvert.DeserializeObject<Models.Diversity>(patchedDiversity);

            // Assert
            Assert.Equal(false, diversity.ConsentToCollectEthnicity);
        }

        [Fact]
        public void DiversityTests_CheckEthnicityIsUpdated_WhenPatchIsCalled()
        {
            var diversityPatch = new Models.DiversityPatch { Ethnicity = Ethnicity.AnyOtherEthnicGroup };

            var patchedDiversity = _diversityPatchService.Patch(_json, diversityPatch);

            var diversity = JsonConvert.DeserializeObject<Models.Diversity>(patchedDiversity);

            // Assert
            Assert.Equal(Ethnicity.AnyOtherEthnicGroup, diversity.Ethnicity);
        }

        [Fact]
        public void DiversityTests_CheckDateAndTimeEthnicityCollectedIsUpdated_WhenPatchIsCalled()
        {
            var diversityPatch = new Models.DiversityPatch { DateAndTimeEthnicityCollected = DateTime.MaxValue };

            var patchedDiversity = _diversityPatchService.Patch(_json, diversityPatch);

            var diversity = JsonConvert.DeserializeObject<Models.Diversity>(patchedDiversity);

            // Assert
            Assert.Equal(DateTime.MaxValue, diversity.DateAndTimeEthnicityCollected);
        }

        [Fact]
        public void DiversityTests_CheckLastModifiedDateIsUpdated_WhenPatchIsCalled()
        {
            var diversityPatch = new Models.DiversityPatch { LastModifiedDate = DateTime.MaxValue };

            var patchedDiversity = _diversityPatchService.Patch(_json, diversityPatch);

            var diversity = JsonConvert.DeserializeObject<Models.Diversity>(patchedDiversity);

            // Assert
            Assert.Equal(DateTime.MaxValue, diversity.LastModifiedDate);
        }

        [Fact]
        public void DiversityTests_CheckLastModifiedByUpdated_WhenPatchIsCalled()
        {
            var diversityPatch = new Models.DiversityPatch { LastModifiedBy = "0000000111" };

            var patchedDiversity = _diversityPatchService.Patch(_json, diversityPatch);

            var diversity = JsonConvert.DeserializeObject<Models.Diversity>(patchedDiversity);

            // Assert
            Assert.Equal("0000000111", diversity.LastModifiedBy);
        }
        
    }
}