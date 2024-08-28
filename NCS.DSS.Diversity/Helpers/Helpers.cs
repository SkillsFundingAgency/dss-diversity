using Microsoft.AspNetCore.Http;
using NCS.DSS.Diversity.Models;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace NCS.DSS.Diversity.Helpers
{
    public class Helper : IHelper
    {
        public async Task UpdateValuesAsync<T>(HttpRequest request, T diversity) where T : IDiversity
        {
            try
            {
                if (request.Body.CanSeek)
                {
                    request.Body.Position = 0;
                }

                string bodyContent;
                using (var reader = new StreamReader(request.Body))
                {
                    bodyContent = await reader.ReadToEndAsync();
                }

                if (string.IsNullOrWhiteSpace(bodyContent))
                {
                    throw new InvalidOperationException("The request body is empty or cannot be read.");
                }

                dynamic bodyJson = JsonSerializer.Deserialize<JsonNode>(bodyContent);

                if (bodyJson == null)
                {
                    throw new InvalidOperationException("Failed to deserialize the request body.");
                }

                var consentToCollectEthnicity = (string)bodyJson["ConsentToCollectEthnicity"] ?? "false";
                var consentToCollectLLDDHealth = (string)bodyJson["ConsentToCollectLLDDHealth"] ?? "false";

                diversity.ConsentToCollectEthnicity = consentToCollectEthnicity.ToLower() == "true" || consentToCollectEthnicity == "1";
                diversity.ConsentToCollectLLDDHealth = consentToCollectLLDDHealth.ToLower() == "true" || consentToCollectLLDDHealth == "1";
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("An error occurred while processing the request.", ex);
            }
        }
    }
}
