using DFC.JSON.Standard;
using NCS.DSS.Diversity.Models;
using Newtonsoft.Json.Linq;

namespace NCS.DSS.Diversity.PatchDiversityHttpTrigger.Service
{
    public class DiversityPatchService : IDiversityPatchService
    {
        private readonly IJsonHelper _jsonHelper;

        public DiversityPatchService(IJsonHelper jsonHelper)
        {
            _jsonHelper = jsonHelper;
        }

        public string Patch(string diversityJson, DiversityPatch diversityPatch)
        {
            if (string.IsNullOrEmpty(diversityJson))
                return null;

            var obj = JObject.Parse(diversityJson);

            if (diversityPatch.ConsentToCollectLLDDHealth.HasValue)
                _jsonHelper.UpdatePropertyValue(obj["ConsentToCollectLLDDHealth"], diversityPatch.ConsentToCollectLLDDHealth);

            if (diversityPatch.LearningDifficultyOrDisabilityDeclaration.HasValue)
                _jsonHelper.UpdatePropertyValue(obj["LearningDifficultyOrDisabilityDeclaration"], diversityPatch.LearningDifficultyOrDisabilityDeclaration);

            if (diversityPatch.PrimaryLearningDifficultyOrDisability.HasValue)
                _jsonHelper.UpdatePropertyValue(obj["PrimaryLearningDifficultyOrDisability"], diversityPatch.PrimaryLearningDifficultyOrDisability);

            if (diversityPatch.SecondaryLearningDifficultyOrDisability.HasValue)
                _jsonHelper.UpdatePropertyValue(obj["SecondaryLearningDifficultyOrDisability"], diversityPatch.SecondaryLearningDifficultyOrDisability);

            if (diversityPatch.DateAndTimeLLDDHealthConsentCollected.HasValue)
                _jsonHelper.UpdatePropertyValue(obj["DateAndTimeLLDDHealthConsentCollected"], diversityPatch.DateAndTimeLLDDHealthConsentCollected);

            if (diversityPatch.ConsentToCollectEthnicity.HasValue)
                _jsonHelper.UpdatePropertyValue(obj["ConsentToCollectEthnicity"], diversityPatch.ConsentToCollectEthnicity);

            if (diversityPatch.Ethnicity.HasValue)
                _jsonHelper.UpdatePropertyValue(obj["Ethnicity"], diversityPatch.Ethnicity);

            if (diversityPatch.DateAndTimeEthnicityCollected.HasValue)
                _jsonHelper.UpdatePropertyValue(obj["DateAndTimeEthnicityCollected"], diversityPatch.DateAndTimeEthnicityCollected);

            if (diversityPatch.LastModifiedDate.HasValue)
                _jsonHelper.UpdatePropertyValue(obj["LastModifiedDate"], diversityPatch.LastModifiedDate);

            if (!string.IsNullOrWhiteSpace(diversityPatch.LastModifiedBy))
                _jsonHelper.UpdatePropertyValue(obj["LastModifiedBy"], diversityPatch.LastModifiedBy);

            return obj.ToString();
        }
    }
}
