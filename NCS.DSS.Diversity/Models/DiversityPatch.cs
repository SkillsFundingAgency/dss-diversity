using System.ComponentModel.DataAnnotations;
using DFC.Swagger.Standard.Annotations;
using NCS.DSS.Diversity.ReferenceData;

namespace NCS.DSS.Diversity.Models
{
    public class DiversityPatch : IDiversity
    {
        [Display(Description = "Indicator to say consent to collect special category LLDD Health data was given by the customer.")]
        [Example(Description = "false")]
        public bool? ConsentToCollectLLDDHealth { get; set; }


        [Display(Description = "Learning Difficulty Or Disability Health Problem Declaration reference data")]
        [Example(Description = "1")]
        public LearningDifficultyOrDisabilityDeclaration? LearningDifficultyOrDisabilityDeclaration { get; set; }

        [Display(Description = "Primary Learning Difficulty Or Disability Heath Problem reference data")]
        [Example(Description = "4")]
        public PrimaryLearningDifficultyOrDisability? PrimaryLearningDifficultyOrDisability { get; set; }

        [Display(Description = "Secondary Learning Difficulty Or Disability Heath Problem reference data")]
        [Example(Description = "5")]
        public SecondaryLearningDifficultyOrDisability? SecondaryLearningDifficultyOrDisability { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Description = "Date and time the LLDD Heath consent was collected from the customer.")]
        [Example(Description = "2018-06-21T17:45:00")]
        public DateTime? DateAndTimeLLDDHealthConsentCollected { get; set; }

        [Display(Description = "Indicator to say consent to collect special category ethnicity data was given by the customer.")]
        [Example(Description = "true")]
        public bool? ConsentToCollectEthnicity { get; set; }

        [Display(Description = "Ethnicity reference data values")]
        [Example(Description = "31")]
        public Ethnicity? Ethnicity { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Description = "Date and time ethnicity data was collected from the customer")]
        [Example(Description = "2018-06-21T14:45:00")]
        public DateTime? DateAndTimeEthnicityCollected { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Description = "Date and time of the last modification to the record.")]
        [Example(Description = "2018-06-21T12:17:00")]
        public DateTime? LastModifiedDate { get; set; }

        [StringLength(10, MinimumLength = 10)]
        [Display(Description = "Identifier of the touchpoint who made the last change to the record")]
        [Example(Description = "0000000001")]
        public string LastModifiedBy { get; set; }

        public void SetDefaultValues()
        {
            if (!LastModifiedDate.HasValue)
                LastModifiedDate = DateTime.UtcNow;
            
            if (!DateAndTimeLLDDHealthConsentCollected.HasValue && ConsentToCollectLLDDHealth.GetValueOrDefault())
                DateAndTimeLLDDHealthConsentCollected = DateTime.UtcNow;

            if (!DateAndTimeEthnicityCollected.HasValue && ConsentToCollectEthnicity.GetValueOrDefault())
                DateAndTimeEthnicityCollected = DateTime.UtcNow;

            if (LearningDifficultyOrDisabilityDeclaration == null && ConsentToCollectLLDDHealth == false)
                LearningDifficultyOrDisabilityDeclaration = ReferenceData.LearningDifficultyOrDisabilityDeclaration.NotProvidedByTheCustomer;

            if (PrimaryLearningDifficultyOrDisability == null && ConsentToCollectLLDDHealth == false)
                PrimaryLearningDifficultyOrDisability = ReferenceData.PrimaryLearningDifficultyOrDisability.NotProvided;

            if (SecondaryLearningDifficultyOrDisability == null && ConsentToCollectLLDDHealth == false)
                SecondaryLearningDifficultyOrDisability = ReferenceData.SecondaryLearningDifficultyOrDisability.NotProvided;

            if (Ethnicity == null && ConsentToCollectEthnicity == false)
                Ethnicity = ReferenceData.Ethnicity.NotProvided;

        }
    }
}
