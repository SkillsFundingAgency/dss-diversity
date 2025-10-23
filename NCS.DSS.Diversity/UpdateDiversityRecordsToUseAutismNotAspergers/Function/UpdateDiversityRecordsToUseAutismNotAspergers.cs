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
using NCS.DSS.Diversity.ReferenceData;

namespace NCS.DSS.Diversity.UpdateDiversityRecordsToUseAutismNotAspergers.Function;

public class UpdateDiversityRecordsToUseAutismNotAspergers
{
    private readonly ILogger<UpdateDiversityRecordsToUseAutismNotAspergers> _logger;
    private readonly Container _diversityContainer;

    public UpdateDiversityRecordsToUseAutismNotAspergers(ILogger<UpdateDiversityRecordsToUseAutismNotAspergers> logger, IOptions<DiversityConfigurationSettings> configOptions, CosmosClient cosmosClient)
    {
        _logger = logger;
        var config = configOptions.Value;

        _diversityContainer = GetContainer(cosmosClient, config.DatabaseId, config.CollectionId);
    }

    private static Container GetContainer(CosmosClient cosmosClient, string databaseId, string collectionId)
            => cosmosClient.GetContainer(databaseId, collectionId);

    [Function("UpdateDiversityRecordsToUseAutismNotAspergers")]
    [ProducesResponseType(typeof(string), 200)]
    [Response(HttpStatusCode = (int)HttpStatusCode.OK, Description = "Successfully altered data", ShowSchema = false)]
    [Response(HttpStatusCode = (int)HttpStatusCode.Unauthorized, Description = "API key is unknown or invalid", ShowSchema = false)]
    [Display(Name = "UpdateDiversityRecordsToUseAutismNotAspergers")]
    public async Task<IActionResult> RunAsync([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req)
    {
        _logger.LogTrace("Function {FunctionName} has been invoked", nameof(UpdateDiversityRecordsToUseAutismNotAspergers));

        if (!req.Headers.ContainsKey("securityKey"))
        {
            return new UnauthorizedResult();
        }

        string securityKey = req.Headers["securityKey"].FirstOrDefault();
        if (securityKey.EndsWith("/"))
        {
            securityKey = securityKey.Substring(0, securityKey.Length - 1);
        }

        if (securityKey != Environment.GetEnvironmentVariable("SecurityKey"))
        {
            _logger.LogWarning("Security key validation failed");
            return new UnauthorizedResult();
        }

        _logger.LogInformation("Successfully validated Security key");

        try
        {
            var query = "SELECT c.id FROM c WHERE c.SecondaryLearningDifficultyOrDisability = 15";
            using var iteratorSecodary = _diversityContainer.GetItemQueryIterator<dynamic>(query);

            while (iteratorSecodary.HasMoreResults)
            {
                foreach (var item in await iteratorSecodary.ReadNextAsync())
                {
                    string id = item.id;

                    PatchItemRequestOptions options = new()
                    {
                        FilterPredicate = "FROM c WHERE c.SecondaryLearningDifficultyOrDisability = 15"
                    };

                    List<PatchOperation> operations = new()
                    {
                        PatchOperation.Replace($"/SecondaryLearningDifficultyOrDisability", 14),
                    };

                    await _diversityContainer.PatchItemAsync<dynamic>(
                        id: id,
                        partitionKey: PartitionKey.None,
                        patchOperations: operations,
                        requestOptions: options
                    );
                }
            }

            _logger.LogTrace("Successfully altered with SecondaryLearningDifficultyOrDisability set to 'Autism' for appropriate records");

            query = "SELECT c.id FROM c WHERE c.PrimaryLearningDifficultyOrDisability = 15";
            using var iteratorPrimary = _diversityContainer.GetItemQueryIterator<dynamic>(query);

            while (iteratorPrimary.HasMoreResults)
            {
                foreach (var item in await iteratorPrimary.ReadNextAsync())
                {
                    string id = item.id;

                    PatchItemRequestOptions options = new()
                    {
                        FilterPredicate = "FROM c WHERE c.PrimaryLearningDifficultyOrDisability = 15"
                    };

                    List<PatchOperation> operations = new()
                    {
                        PatchOperation.Replace($"/PrimaryLearningDifficultyOrDisability", 14),
                    };

                    await _diversityContainer.PatchItemAsync<dynamic>(
                        id: id,
                        partitionKey: PartitionKey.None,
                        patchOperations: operations,
                        requestOptions: options
                    );
                }
            }
            _logger.LogTrace("Successfully altered with PrimaryLearningDifficultyOrDisability set to 'Autism' for appropriate records");
            _logger.LogTrace("Function {FunctionName} has finished invoking", nameof(UpdateDiversityRecordsToUseAutismNotAspergers));

            return new OkObjectResult("Success");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while updating diversity records.");
            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        }
    }
}
