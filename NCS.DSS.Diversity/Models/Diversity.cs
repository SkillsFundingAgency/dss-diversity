using System;
using System.ComponentModel.DataAnnotations;

namespace NCS.DSS.Diversity.Models
{
    public class Diversity
    {
        [Display(Description = "Unique identifier for a diversity record")]
        public Guid DiversityId { get; set; }

        [Required]
        [Display(Description = "Unique identifier of a customer.")]
        public Guid CustomerId { get; set; }

        [Required]
        [Display(Description = "Indicator to say consent to collect special category LLDD Health data was given by the customer.")]
        public bool ConsentToCollectLLDDHealth { get; set; }

        [Required]
        [Display(Description = "LLDD Health Problem Declaration reference data.")]
        public int LLDDHealthProblemDeclarationId { get; set; }

        [Required]
        [Display(Description = "Primary LLDD Heath Problem reference data.")]
        public int PrimaryLLDDHeathProblemId { get; set; }

        [Required]
        [Display(Description = "Secondary LLDD Heath Problem reference data.")]
        public int SecondaryLLDDHeathProblemId { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Description = "Date and time the LLDD Heath consent was collected from the customer.")]
        public DateTime DateAndTimeLLDDHealthConsentCollected { get; set; }

        [Display(Description = "Indicator to say consent to collect special category ethnicity data was given by the customer.")]
        public bool ConsentToCollectEthnicity { get; set; }

        [Required]
        [Display(Description = "Ethnicity reference data.")]
        public Guid EthnicityId { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Description = "Date and time ethnicity data was collected from the customer")]
        public DateTime DateAndTimeEthnicityCollected { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Description = "Date and time of the last modification to the record.")]
        public DateTime LastModifiedDate { get; set; }

        [Display(Description = "Identifier of the touchpoint who made the last change to the record")]
        public Guid LastModifiedBy { get; set; }
    } 
}
