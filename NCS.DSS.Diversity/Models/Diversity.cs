using System;
using System.ComponentModel.DataAnnotations;
using NCS.DSS.Diversity.Annotations;
using NCS.DSS.Diversity.ReferenceData;

namespace NCS.DSS.Diversity.Models
{
    public class Diversity
    {
        [Display(Description = "Unique identifier for a diversity record")]
        [Example(Description = "b8592ff8-af97-49ad-9fb2-e5c3c717fd85")]
        [Newtonsoft.Json.JsonProperty(PropertyName = "id")]
        public Guid? DiversityId { get; set; }

        [Required]
        [Display(Description = "Unique identifier of a customer.")]
        [Example(Description = "2730af9c-fc34-4c2b-a905-c4b584b0f379")]
        public Guid CustomerId { get; set; }

        [Required]
        [Display(Description = "Indicator to say consent to collect special category LLDD Health data was given by the customer.")]
        [Example(Description = "false")]
        public bool ConsentToCollectLLDDHealth { get; set; }

        [Required]
        [Display(Description = "Learning Difficulty Or Disability Health Problem Declaration reference data.")]
        [Example(Description = "1")]
        public LearningDifficultyOrDisabilityDeclaration LearningDifficultyOrDisabilityDeclaration { get; set; }

        [Required]
        [Display(Description = "Primary Learning Difficulty Or Disability Heath Problem reference data.")]
        [Example(Description = "4")]
        public PrimaryLearningDifficultyOrDisability PrimaryLearningDifficultyOrDisability { get; set; }

        [Required]
        [Display(Description = "Secondary Learning Difficulty Or Disability Heath Problem reference data.")]
        [Example(Description = "5")]
        public SecondaryLearningDifficultyOrDisability SecondaryLearningDifficultyOrDisability { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Description = "Date and time the LLDD Heath consent was collected from the customer.")]
        [Example(Description = "2018-06-21T17:45:00")]
        public DateTime DateAndTimeLLDDHealthConsentCollected { get; set; }

        [Display(Description = "Indicator to say consent to collect special category ethnicity data was given by the customer.")]
        [Example(Description = "true")]
        public bool ConsentToCollectEthnicity { get; set; }

        [Required]
        [Display(Description = "Ethnicity reference data.")]
        [Example(Description = "31")]
        public Ethnicity Ethnicity { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Description = "Date and time ethnicity data was collected from the customer")]
        [Example(Description = "2018-06-21T14:45:00")]
        public DateTime DateAndTimeEthnicityCollected { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Description = "Date and time of the last modification to the record.")]
        [Example(Description = "2018-06-21T12:17:00")]
        public DateTime LastModifiedDate { get; set; }

        [Display(Description = "Identifier of the touchpoint who made the last change to the record")]
        [Example(Description = "d1307d77-af23-4cb4-b600-a60e04f8c3df")]
        public Guid LastModifiedBy { get; set; }
    } 
}
