using System;
using NCS.DSS.Diversity.ReferenceData;

namespace NCS.DSS.Diversity.Models
{
    public interface IDiversity
    {
        bool? ConsentToCollectLLDDHealth { get; set; }
        LearningDifficultyOrDisabilityDeclaration? LearningDifficultyOrDisabilityDeclaration { get; set; }
        PrimaryLearningDifficultyOrDisability? PrimaryLearningDifficultyOrDisability { get; set; }
        SecondaryLearningDifficultyOrDisability? SecondaryLearningDifficultyOrDisability { get; set; }
        DateTime? DateAndTimeLLDDHealthConsentCollected { get; set; }
        bool? ConsentToCollectEthnicity { get; set; }
        Ethnicity? Ethnicity { get; set; }
        DateTime? DateAndTimeEthnicityCollected { get; set; }
        DateTime? LastModifiedDate { get; set; }
        Guid? LastModifiedBy { get; set; }

        void SetDefaultValues();
    }
}