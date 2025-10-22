using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NCS.DSS.Diversity.Models;
using System.ComponentModel.DataAnnotations;
using System.Net;
using DFC.Swagger.Standard.Annotations;

namespace NCS.DSS.Diversity;

public class Rollback241283
{
    private readonly ILogger<Rollback241283> _logger;
    private readonly Container _diversityContainer;

    public Rollback241283(ILogger<Rollback241283> logger, IOptions<DiversityConfigurationSettings> configOptions, CosmosClient cosmosClient)
    {
        _logger = logger;
        var config = configOptions.Value;

        _diversityContainer = GetContainer(cosmosClient, config.DatabaseId, config.CollectionId);
    }

    private static Container GetContainer(CosmosClient cosmosClient, string databaseId, string collectionId)
            => cosmosClient.GetContainer(databaseId, collectionId);

    [Function("Rollback241283")]
    [ProducesResponseType(typeof(string), 200)]
    [Response(HttpStatusCode = (int)HttpStatusCode.OK, Description = "Successfully altered data", ShowSchema = false)]
    [Response(HttpStatusCode = (int)HttpStatusCode.Unauthorized, Description = "API key is unknown or invalid", ShowSchema = false)]
    [Display(Name = "Rollback241283")]
    public async Task<IActionResult> RunAsync([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req)
    {
        _logger.LogInformation("Function {FunctionName} has been invoked", nameof(Rollback241283));

        try
        {
            var secondaryIds = new List<string>();
            using (var reader = new StreamReader("SecondaryIDsAspergers.csv"))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    var columns = line.Split(',');
                    if (columns.Length > 0)
                    {
                        if (!columns[0].Contains("sep") && !columns[0].Contains("id"))
                        {
                            string id = columns[0].Trim(new char[] { '/', '"' });
                            secondaryIds.Add(id);
                        }
                    }
                }
            }

            foreach (string id in secondaryIds) {

                List<PatchOperation> operations = new()
                    {
                        PatchOperation.Replace($"/SecondaryLearningDifficultyOrDisability", 15),
                    };

                await _diversityContainer.PatchItemAsync<dynamic>(
                    id: id,
                    partitionKey: PartitionKey.None,
                    patchOperations: operations
                );

                _logger.LogInformation("Successfully altered with SecondaryLearningDifficultyOrDisability set to 'Autism' for appropriate records"); 
            
            }

            var primaryIds = new List<string>();
            using (var reader = new StreamReader("PrimaryIDsAspergers.csv"))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    var columns = line.Split(',');
                    if (columns.Length > 0)
                    {
                        if (!columns[0].Contains("sep") && !columns[0].Contains("id"))
                        {
                            string id = columns[0].Trim(new char[] { '/', '"' });
                            primaryIds.Add(id);
                        }
                    }
                }
            }
            foreach (string id in primaryIds)
            {
                List<PatchOperation> operations2 = new()
                    {
                        PatchOperation.Replace($"/PrimaryLearningDifficultyOrDisability", 15),
                    };

                await _diversityContainer.PatchItemAsync<dynamic>(
                    id: id,
                    partitionKey: PartitionKey.None,
                    patchOperations: operations2
                );
                _logger.LogInformation("Successfully altered with PrimaryLearningDifficultyOrDisability set to 'Autism' for appropriate records");
            }
            _logger.LogInformation("Function {FunctionName} has finished invoking", nameof(UpdateDiversityRecordsToUseAutismNotAspergers));

            return new OkObjectResult("Success");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while updating diversity records.");
            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        }
    }
}
