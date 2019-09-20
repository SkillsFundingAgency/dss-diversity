using System;
using System.ComponentModel.DataAnnotations;
using NCS.DSS.Diversity.Annotations;
using NCS.DSS.Diversity.ReferenceData;

namespace NCS.DSS.Diversity.Models
{
    public class DiversityPatch : IDiversity
    {
        [Display(Description = "Indicator to say consent to collect special category LLDD Health data was given by the customer.")]
        [Example(Description = "false")]
        public bool? ConsentToCollectLLDDHealth { get; set; }


        [Display(Description = "Learning Difficulty Or Disability Health Problem Declaration reference data   :   " +
                                "1 - Customer considers themselves to have a learning difficulty and / or health problem, " +
                                "2 - Customer does not consider themselves to have a learning difficulty and / or health problem, " +
                                "9 - Not provided by the customer ")]
        [Example(Description = "1")]
        public LearningDifficultyOrDisabilityDeclaration? LearningDifficultyOrDisabilityDeclaration { get; set; }

        [Display(Description = "Primary Learning Difficulty Or Disability Heath Problem reference data   :   " +
                                "4 - Visual impairment,  " +
                                "5 - Hearing impairment,  " +
                                "6 - Disability affecting mobility,  " +
                                "7 - Profound complex disabilities,  " +
                                "8 - Social and emotional difficulties,  " +
                                "9 - Mental health difficulty,  " +
                                "10 - Moderate learning difficulty,  " +
                                "11 - Severe learning difficulty,  " +
                                "12 - Dyslexia,  " +
                                "13 - Dyscalculia,  " +
                                "14 - Autism spectrum disorder,  " +
                                "15 - Asperger's syndrome ,  " +
                                "16 - Temporary disability after illness(for example post - viral) or accident,  " +
                                "17 - Speech, Language and Communication Needs,  " +
                                "93 - Other physical disability,  " +
                                "94 - Other specific learning difficulty(e.g.Dyspraxia),  " +
                                "95 - Other medical condition(for example epilepsy, asthma, diabetes),  " +
                                "96 - Other learning difficulty,  " +
                                "97 - Other disability,  " +
                                "98 - Prefer not to say,  " +
                                "99 - Not provided")]
        [Example(Description = "4")]
        public PrimaryLearningDifficultyOrDisability? PrimaryLearningDifficultyOrDisability { get; set; }

        [Display(Description = "Secondary Learning Difficulty Or Disability Heath Problem reference data   :   " +
                                "4 - Visual impairment,  " +
                                "5 - Hearing impairment,  " +
                                "6 - Disability affecting mobility,  " +
                                "7 - Profound complex disabilities,  " +
                                "8 - Social and emotional difficulties,  " +
                                "9 - Mental health difficulty,  " +
                                "10 - Moderate learning difficulty,  " +
                                "11 - Severe learning difficulty,  " +
                                "12 - Dyslexia,  " +
                                "13 - Dyscalculia,  " +
                                "14 - Autism spectrum disorder,  " +
                                "15 - Asperger's syndrome ,  " +
                                "16 - Temporary disability after illness(for example post - viral) or accident,  " +
                                "17 - Speech, Language and Communication Needs,  " +
                                "93 - Other physical disability,  " +
                                "94 - Other specific learning difficulty(e.g.Dyspraxia),  " +
                                "95 - Other medical condition(for example epilepsy, asthma, diabetes),  " +
                                "96 - Other learning difficulty,  " +
                                "97 - Other disability,  " +
                                "98 - Prefer not to say,  " +
                                "99 - Not provided")]
        [Example(Description = "5")]
        public SecondaryLearningDifficultyOrDisability? SecondaryLearningDifficultyOrDisability { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Description = "Date and time the LLDD Heath consent was collected from the customer.")]
        [Example(Description = "2018-06-21T17:45:00")]
        public DateTime? DateAndTimeLLDDHealthConsentCollected { get; set; }

        [Display(Description = "Indicator to say consent to collect special category ethnicity data was given by the customer.")]
        [Example(Description = "true")]
        public bool? ConsentToCollectEthnicity { get; set; }

        [Display(Description = "Ethnicity reference data values   :   " +
                                "31 - English / Welsh / Scottish / Northern Irish / British,   " +
                                "32 - Irish,   " +
                                "33 - Gypsy or Irish Traveller,   " +
                                "34 - Any Other White background,   " +
                                "35 - White and Black Caribbean,   " +
                                "36 - White and Black African,   " +
                                "37 - White and Asian,   " +
                                "38 - Any Other Mixed / multiple ethnic background,   " +
                                "39 - Indian,   " +
                                "40 - Pakistani,   " +
                                "41 - Bangladeshi,   " +
                                "42 - Chinese,   " +
                                "43 - Any other Asian background,   " +
                                "44 - African,   " +
                                "45 - Caribbean,   " +
                                "46 - Any other Black / African / Caribbean background,   " +
                                "47 - Arab,   " +
                                "98 - Any other ethnic group,   " +
                                "99 - Not provided")]
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
        }
    }
}
