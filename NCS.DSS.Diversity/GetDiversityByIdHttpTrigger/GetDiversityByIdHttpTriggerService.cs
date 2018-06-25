using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NCS.DSS.Diversity.ReferenceData;

namespace NCS.DSS.Diversity.GetDiversityByIdHttpTrigger
{
    public class GetDiversityByIdHttpTriggerService
    {

        public async Task<Models.Diversity> GetDiversity(Guid diversityId)
        {
            var diversities = CreateTempDiversities();
            var result = diversities.FirstOrDefault(a => a.DiversityId == diversityId);
            return await Task.FromResult(result);
        }

        public List<Models.Diversity> CreateTempDiversities()
        {
            var diversityList = new List<Models.Diversity>
            {
               new Models.Diversity
               {
                   DiversityId = Guid.Parse("b11676c9-e17f-4658-b35e-3fdc23b4adb3"),
                   CustomerId = Guid.Parse("c4456c9b-e850-4877-8206-7c14f4930397"),
                   ConsentToCollectLLDDHealth = true,
                   DateAndTimeLLDDHealthConsentCollected = DateTime.UtcNow,
                   LLDDHealthProblemDeclaration = LLDDHealthProblemDeclaration.CustomerConsidersThemselvesToHaveALearningDifficultyAndOrHealthProblem,
                   PrimaryLLDDHeathProblem = PrimaryLLDDHeathProblem.AspergersSyndrome,
                   ConsentToCollectEthnicity = false,
                   SecondaryLLDDHeathProblem = SecondaryLLDDHeathProblem.AutismSpectrumDisorder,
                   Ethnicity = Ethnicity.African,
                   LastModifiedDate = DateTime.UtcNow,
                   LastModifiedBy = Guid.Empty
               },
                new Models.Diversity
                {
                    DiversityId = Guid.Parse("2f9e4ece-95d9-444b-8833-b433f5fd2190"),
                    CustomerId = Guid.Parse("dc9f5f38-5122-4d90-b79f-788bc40499d9"),
                    ConsentToCollectLLDDHealth = false,
                    LLDDHealthProblemDeclaration = LLDDHealthProblemDeclaration.CustomerDoesNotConsiderThemselvesToHaveALearningDifficultyAndOrHealthProblem,
                    PrimaryLLDDHeathProblem = PrimaryLLDDHeathProblem.AutismSpectrumDisorder,
                    ConsentToCollectEthnicity = true,
                    DateAndTimeEthnicityCollected = DateTime.UtcNow,
                    SecondaryLLDDHeathProblem = SecondaryLLDDHeathProblem.Dyscalculia,
                    Ethnicity = Ethnicity.Chinese,
                    LastModifiedDate = DateTime.UtcNow,
                    LastModifiedBy = Guid.Empty
                },
                new Models.Diversity
                {
                    DiversityId = Guid.Parse("67fefdd2-6e1c-48b8-b9be-2adc5d0d48c4"),
                    CustomerId = Guid.Parse("f5b7ea25-381c-4219-883a-635e7d867657"),
                    ConsentToCollectLLDDHealth = true,
                    DateAndTimeLLDDHealthConsentCollected = DateTime.UtcNow,
                    LLDDHealthProblemDeclaration = LLDDHealthProblemDeclaration.NotProvidedByTheCustomer,
                    PrimaryLLDDHeathProblem = PrimaryLLDDHeathProblem.NotProvided,
                    ConsentToCollectEthnicity = true,
                    DateAndTimeEthnicityCollected = DateTime.UtcNow,
                    SecondaryLLDDHeathProblem = SecondaryLLDDHeathProblem.TemporaryDisabilityAfterIllnessOrAccident,
                    Ethnicity = Ethnicity.GypsyIrishTraveller,
                    LastModifiedDate = DateTime.UtcNow,
                    LastModifiedBy = Guid.Empty
                }
            };

            return diversityList;
        }

    }
}