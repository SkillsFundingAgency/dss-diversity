using DFC.JSON.Standard;
using Microsoft.Extensions.Logging;
using NCS.DSS.Diversity.Models;
using Newtonsoft.Json.Linq;

namespace NCS.DSS.Diversity.PatchDiversityHttpTrigger.Service
{
    public class DiversityPatchService : IDiversityPatchService
    {
        private readonly IJsonHelper _jsonHelper;
        private readonly ILogger<DiversityPatchService> _logger;

        public DiversityPatchService(IJsonHelper jsonHelper, ILogger<DiversityPatchService> logger)
        {
            _jsonHelper = jsonHelper;
            _logger = logger;
        }

        public string Patch(string diversityJson, DiversityPatch diversityPatch)
        {
            _logger.LogInformation("Started updating diversity json object with PATCH request");

            if (string.IsNullOrEmpty(diversityJson))
            {
                _logger.LogWarning("Invalid diversityJson object provided. diversity json is either empty or null");
                return null;
            }

            var obj = JObject.Parse(diversityJson);

            if (diversityPatch.ConsentToCollectLLDDHealth.HasValue)
            {
                _jsonHelper.UpdatePropertyValue(obj["ConsentToCollectLLDDHealth"], diversityPatch.ConsentToCollectLLDDHealth);
            }

            if (diversityPatch.LearningDifficultyOrDisabilityDeclaration.HasValue)
            {
                _jsonHelper.UpdatePropertyValue(obj["LearningDifficultyOrDisabilityDeclaration"], diversityPatch.LearningDifficultyOrDisabilityDeclaration);
            }

            if (diversityPatch.PrimaryLearningDifficultyOrDisability.HasValue)
            {
                _jsonHelper.UpdatePropertyValue(obj["PrimaryLearningDifficultyOrDisability"], diversityPatch.PrimaryLearningDifficultyOrDisability);
            }

            if (diversityPatch.SecondaryLearningDifficultyOrDisability.HasValue)
            {
                _jsonHelper.UpdatePropertyValue(obj["SecondaryLearningDifficultyOrDisability"], diversityPatch.SecondaryLearningDifficultyOrDisability);
            }

            if (diversityPatch.DateAndTimeLLDDHealthConsentCollected.HasValue)
            {
                _jsonHelper.UpdatePropertyValue(obj["DateAndTimeLLDDHealthConsentCollected"], diversityPatch.DateAndTimeLLDDHealthConsentCollected);
            }

            if (diversityPatch.ConsentToCollectEthnicity.HasValue)
            {
                _jsonHelper.UpdatePropertyValue(obj["ConsentToCollectEthnicity"], diversityPatch.ConsentToCollectEthnicity);
            }

            if (diversityPatch.Ethnicity.HasValue)
            {
                _jsonHelper.UpdatePropertyValue(obj["Ethnicity"], diversityPatch.Ethnicity);
            }

            if (diversityPatch.DateAndTimeEthnicityCollected.HasValue)
            {
                _jsonHelper.UpdatePropertyValue(obj["DateAndTimeEthnicityCollected"], diversityPatch.DateAndTimeEthnicityCollected);
            }

            if (diversityPatch.LastModifiedDate.HasValue)
            {
                _jsonHelper.UpdatePropertyValue(obj["LastModifiedDate"], diversityPatch.LastModifiedDate);
            }

            if (!string.IsNullOrWhiteSpace(diversityPatch.LastModifiedBy))
            {
                _jsonHelper.UpdatePropertyValue(obj["LastModifiedBy"], diversityPatch.LastModifiedBy);
            }

            _logger.LogInformation("Completed updating diversity json object with PATCH request.");
            return obj.ToString();
        }
    }
}
