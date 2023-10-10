using Microsoft.AspNetCore.Http;
using NCS.DSS.Diversity.Models;
using Newtonsoft.Json;
using System.IO;

namespace NCS.DSS.Diversity.Helpers
{
    public class Helper : IHelper
    {
        public void UpdateValues<T>(HttpRequest request, T diversity) where T : IDiversity
        {
            request.Body.Position = 0;

            dynamic bodyContent = JsonConvert.DeserializeObject(new StreamReader(request.Body).ReadToEnd());
            var consentToCollectEthnicity = (string)bodyContent.ConsentToCollectEthnicity;
            var consentToCollectLLDDHealth = (string)bodyContent.ConsentToCollectLLDDHealth;

            diversity.ConsentToCollectEthnicity = consentToCollectEthnicity.ToLower() == "true" || consentToCollectEthnicity == "1" ? true : false;

            diversity.ConsentToCollectLLDDHealth = consentToCollectLLDDHealth.ToLower() == "true" || consentToCollectLLDDHealth == "1" ? true : false;
        }
    }
}
