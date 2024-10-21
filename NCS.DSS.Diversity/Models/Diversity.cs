﻿using DFC.Swagger.Standard.Annotations;
using NCS.DSS.Diversity.ReferenceData;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace NCS.DSS.Diversity.Models
{
    public class Diversity : IDiversity
    {
        [Display(Description = "Unique identifier for a diversity record")]
        [Example(Description = "b8592ff8-af97-49ad-9fb2-e5c3c717fd85")]
        [Newtonsoft.Json.JsonProperty(PropertyName = "id")]
        public Guid? DiversityId { get; set; }

        [Display(Description = "Unique identifier of a customer.")]
        [Example(Description = "2730af9c-fc34-4c2b-a905-c4b584b0f379")]
        public Guid? CustomerId { get; set; }

        [Required]
        [Display(Description = "Indicator to say consent to collect special category LLDD Health data was given by the customer")]
        [Example(Description = "false")]
        public bool? ConsentToCollectLLDDHealth { get; set; }

        [Required]
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

        [Required]
        [Display(Description = "Indicator to say consent to collect special category ethnicity data was given by the customer.")]
        [Example(Description = "true")]
        public bool? ConsentToCollectEthnicity { get; set; }

        [Required]
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

        [JsonIgnore]
        public string CreatedBy { get; set; }


        public void SetDefaultValues()
        {
            if (!LastModifiedDate.HasValue)
                LastModifiedDate = DateTime.UtcNow;

            if (LearningDifficultyOrDisabilityDeclaration == null)
                LearningDifficultyOrDisabilityDeclaration = ReferenceData.LearningDifficultyOrDisabilityDeclaration.NotProvidedByTheCustomer;

            if (PrimaryLearningDifficultyOrDisability == null)
                PrimaryLearningDifficultyOrDisability = ReferenceData.PrimaryLearningDifficultyOrDisability.NotProvided;

            if (SecondaryLearningDifficultyOrDisability == null)
                SecondaryLearningDifficultyOrDisability = ReferenceData.SecondaryLearningDifficultyOrDisability.NotProvided;

            if (Ethnicity == null)
                Ethnicity = ReferenceData.Ethnicity.NotProvided;

            if (!DateAndTimeLLDDHealthConsentCollected.HasValue && ConsentToCollectLLDDHealth.GetValueOrDefault())
                DateAndTimeLLDDHealthConsentCollected = DateTime.UtcNow;

            if (!DateAndTimeEthnicityCollected.HasValue && ConsentToCollectEthnicity.GetValueOrDefault())
                DateAndTimeEthnicityCollected = DateTime.UtcNow;
        }

        public void SetIds(Guid customerId, string touchpointId)
        {
            DiversityId = Guid.NewGuid();
            CustomerId = customerId;
            LastModifiedBy = touchpointId;
            CreatedBy = touchpointId;
        }

    }
}
