using Microsoft.AspNetCore.Http;
using NCS.DSS.Diversity.Models;
using Newtonsoft.Json;
using System.IO;

namespace NCS.DSS.Diversity.Helpers
{
    public class Helper : IHelper
    {
        public async Task UpdateValuesAsync<T>(HttpRequest request, T diversity) where T : IDiversity
        {
            try
            {
                // Ensure the request body can be read multiple times by enabling buffering
                request.EnableBuffering();

                if (request.Body.CanSeek)
                {
                    request.Body.Position = 0;
                }

                string bodyContent;
                using (var reader = new StreamReader(request.Body, leaveOpen: true))
                {
                    bodyContent = await reader.ReadToEndAsync();
                }

                if (string.IsNullOrWhiteSpace(bodyContent))
                {
                    throw new InvalidOperationException("The request body is empty or cannot be read.");
                }

                dynamic bodyJson = JsonConvert.DeserializeObject(bodyContent);

                if (bodyJson == null)
                {
                    throw new InvalidOperationException("Failed to deserialize the request body.");
                }

                var consentToCollectEthnicity = (string)bodyJson.ConsentToCollectEthnicity ?? "false";
                var consentToCollectLLDDHealth = (string)bodyJson.ConsentToCollectLLDDHealth ?? "false";

                diversity.ConsentToCollectEthnicity = consentToCollectEthnicity.ToLower() == "true" || consentToCollectEthnicity == "1" ? true : false;
                diversity.ConsentToCollectLLDDHealth = consentToCollectLLDDHealth.ToLower() == "true" || consentToCollectLLDDHealth == "1" ? true : false;

                // Reset the stream position to 0 if needed for further processing after this method
                request.Body.Position = 0;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("An error occurred while processing the request.", ex);
            }
        }
    }
}
